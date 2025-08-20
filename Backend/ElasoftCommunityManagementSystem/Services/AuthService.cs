using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ElasoftCommunityManagementSystem.Dtos.AuthDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class LoginResult
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required string Role { get; set; }
        public int UserId { get; set; }
        public required UserModel User { get; set; }
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtTokenGeneratorService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ITwoFactorService _twoFactorService;
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled
        );

        public AuthService(
            AppDbContext context,
            JwtTokenGeneratorService tokenService,
            IConfiguration configuration,
            ITwoFactorService twoFactorService
        )
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _twoFactorService = twoFactorService;
        }

        public async Task<(bool Success, string Message, string TempPassword)> Register(RegisterDto dto)
        {
            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Surname))
                    throw new ValidationException("Ad ve soyad alanları zorunludur.");

                if (!EmailRegex.IsMatch(dto.Email))
                    throw new ValidationException("Geçersiz email formatı.");

                if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                    throw new ValidationException("Telefon numarası zorunludur.");

                if (string.IsNullOrWhiteSpace(dto.Nationality))
                    throw new ValidationException("Uyruk alanı zorunludur.");

                if (string.IsNullOrWhiteSpace(dto.Gender))
                    throw new ValidationException("Cinsiyet alanı zorunludur.");

                // Check if email exists
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                    throw new BusinessException("Bu email adresi zaten kullanımda.");

                // Generate and hash temporary password
                string tempPassword = GenerateRandomPassword();
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(tempPassword);

                // Create new user
                var user = new UserModel
                {
                    Name = dto.Name.Trim(),
                    Surname = dto.Surname.Trim(),
                    Email = dto.Email.ToLower().Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    SchoolNumber = dto.SchoolNumber?.Trim(),
                    Nationality = dto.Nationality.Trim(),
                    Gender = dto.Gender.Trim(),
                    DisabilityStatus = dto.DisabilityStatus?.Trim(),
                    Password = passwordHash,
                    Role = "user", // Değiştirildi: sadece "admin" ve "user" rolleri olacak
                    CreatedAt = DateTime.UtcNow,
                    TwoFactorEnabled = false,
                    TwoFactorSecret = null,
                };

                // Save to database
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Try to send welcome email with temporary password, but don't fail if it doesn't work
                    try
                    {
                        await SendPasswordEmail(dto.Email, tempPassword);
                        Console.WriteLine(
                            $"Password email sent to {dto.Email} with password: {tempPassword}"
                        );
                    }
                    catch (Exception ex)
                    {
                        // Log the error but continue with registration
                        Console.WriteLine(
                            $"Email sending failed: {ex.Message}. Using password: {tempPassword}"
                        );
                        // Don't throw the exception so registration can still complete
                    }

                    await transaction.CommitAsync();

                    // Include the password in the success message during development
                    return (true, $"Kayıt başarılı. Geçici şifreniz:", tempPassword);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Registration failed: {ex.Message}");
                    throw;
                }
            }
            catch (ValidationException ex)
            {
                Console.WriteLine($"Validation error: {ex.Message}");
                return (false, ex.Message, string.Empty);
            }
            catch (BusinessException ex)
            {
                Console.WriteLine($"Business error: {ex.Message}");
                return (false, ex.Message, string.Empty);
            }
            catch (Exception ex)
            {
                // Log the exception here
                Console.WriteLine($"Unexpected error: {ex.Message}");
                return (
                    false,
                    "Kayıt işlemi sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    string.Empty
                );
            }
        }

        private string GenerateRandomPassword()
        {
            const string validChars =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var random = new Random();
            var password = new StringBuilder();

            // En az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içeren 8 karakterli şifre
            password.Append(validChars[random.Next(26)]); // Küçük harf
            password.Append(validChars[random.Next(26, 52)]); // Büyük harf
            password.Append(validChars[random.Next(52, 62)]); // Rakam
            password.Append(validChars[random.Next(62, validChars.Length)]); // Özel karakter

            // Kalan karakterleri rastgele ekle
            for (int i = 4; i < 8; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            // Karakterlerin sırasını karıştır
            return new string(
                password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray()
            );
        }

        private async Task SendPasswordEmail(string email, string password)
        {
            try
            {
                var emailSettings = _configuration.GetSection("Email");

                if (
                    string.IsNullOrEmpty(emailSettings["SmtpServer"])
                    || string.IsNullOrEmpty(emailSettings["Username"])
                    || string.IsNullOrEmpty(emailSettings["Password"])
                )
                {
                    // Email settings are not configured
                    throw new BusinessException("Email settings not configured properly");
                }

                var smtpClient = new SmtpClient(emailSettings["SmtpServer"])
                {
                    Port = int.Parse(emailSettings["SmtpPort"]),
                    Credentials = new NetworkCredential(
                        emailSettings["Username"],
                        emailSettings["Password"]
                    ),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        emailSettings["SenderEmail"],
                        emailSettings["SenderName"] ?? "Elasoft Topluluk Yönetim Sistemi"
                    ),
                    Subject = "Hoş Geldiniz - Giriş Bilgileriniz",
                    IsBodyHtml = true,
                    Body =
                        $@"
                        <h2>Hoş Geldiniz!</h2>
                        <p>Elasoft Topluluk Yönetim Sistemine kaydınız başarıyla oluşturuldu.</p>
                        <p>Giriş yapmak için aşağıdaki bilgileri kullanabilirsiniz:</p>
                        <p><strong>E-posta:</strong> {email}</p>
                        <p><strong>Geçici Şifre:</strong> {password}</p>
                        <p><strong>Önemli:</strong> İlk girişinizde şifrenizi değiştirmeniz önerilir.</p>
                        <p>Güvenliğiniz için bu e-postayı aldıktan sonra silin.</p>",
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine($"Email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                // Log the error but don't throw it - we'll still allow registration to succeed
                Console.WriteLine($"Failed to send email: {ex.Message}");
            }
        }

        public async Task<LoginResult?> Login(string email, string password)
        {
            try
            {
                Console.WriteLine($"Login attempt for email: {email}");

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (user == null)
                {
                    Console.WriteLine("User not found with this email");
                    throw new BusinessException("Email veya şifre hatalı.");
                }

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                {
                    Console.WriteLine("Password verification failed");
                    throw new BusinessException("Email veya şifre hatalı.");
                }

                // Check if role is valid
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "öğrenci";
                    await _context.SaveChangesAsync();
                }

                // Generate access token
                var accessToken = _tokenService.GenerateToken(user);

                // Generate refresh token
                var refreshToken = GenerateRefreshToken();
                var refreshTokenEntity = new RefreshTokenModel
                {
                    UserId = user.UserId,
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false,
                };

                _context.RefreshTokens.Add(refreshTokenEntity);
                await _context.SaveChangesAsync();

                return new LoginResult
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    Role = user.Role,
                    UserId = user.UserId,
                    User = user,
                };
            }
            catch (BusinessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected login error: {ex.Message}");
                throw new BusinessException("Giriş sırasında beklenmeyen bir hata oluştu.");
            }
        }

        public async Task<LoginResult> RefreshToken(string refreshToken)
        {
            var storedToken = await _context
                .RefreshTokens.Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            if (
                storedToken == null
                || storedToken.IsRevoked
                || storedToken.ExpiryDate < DateTime.UtcNow
            )
                throw new UnauthorizedAccessException("Invalid refresh token.");

            var user = storedToken.User;
            var newAccessToken = _tokenService.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            storedToken.IsRevoked = true;
            var newRefreshTokenEntity = new RefreshTokenModel
            {
                UserId = user.UserId,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false,
            };

            _context.RefreshTokens.Add(newRefreshTokenEntity);
            await _context.SaveChangesAsync();

            return new LoginResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = user.Role ?? throw new BusinessException("User role not set"),
                UserId = user.UserId,
                User = user,
            };
        }

        public async Task RevokeRefreshToken(string refreshToken)
        {
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(r =>
                r.Token == refreshToken
            );
            if (storedToken != null)
            {
                storedToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateRefreshToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }

        public async Task<(string qrCodeUrl, string secretKey)> Enable2FA(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new BusinessException("User not found");

            // Generate secret key for 2FA using TwoFactorService
            var secretKey = _twoFactorService.GenerateSecretKey();
            user.TwoFactorSecret = secretKey;
            user.TwoFactorEnabled = false; // Will be set to true after verification
            user.LastTwoFactorSetup = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Generate QR code URL using TwoFactorService
            var qrCodeUrl = _twoFactorService.GenerateQrCodeUrl(user.Email, secretKey);
            
            return (qrCodeUrl, secretKey);
        }

        public async Task<bool> Verify2FA(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
                throw new BusinessException("Invalid user or 2FA not enabled");

            var isValid = _twoFactorService.ValidateCode(user.TwoFactorSecret, code);
            if (isValid)
            {
                user.TwoFactorEnabled = true;
                await _context.SaveChangesAsync();
            }
            return isValid;
        }

        public async Task Disable2FA(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new BusinessException("User not found");

            user.TwoFactorSecret = null;
            user.TwoFactorEnabled = false;
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Validate2FA(int userId, string code)
        {
            var user = await _context.Users.FindAsync(userId);
            if (
                user == null
                || !user.TwoFactorEnabled
                || string.IsNullOrEmpty(user.TwoFactorSecret)
            )
                throw new BusinessException("Invalid user or 2FA not enabled");

            return ValidateTotp(user.TwoFactorSecret, code);
        }

        private bool ValidateTotp(string secret, string code)
        {
            return _twoFactorService.ValidateCode(secret, code);
        }

        public async Task RequestPasswordReset(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                throw new BusinessException("User not found");

            // Generate reset token
            var resetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            user.PasswordResetToken = resetToken;
            user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            // TODO: Send reset token via email
        }

        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Email == email
                && u.PasswordResetToken == token
                && u.PasswordResetTokenExpiry > DateTime.UtcNow
            );

            if (user == null)
                throw new BusinessException("Invalid or expired reset token");

            // Sadece şifre ve tokenları güncelle
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpiry = null;

            // Sadece bu alanların güncellendiğinden emin ol
            _context.Entry(user).Property(x => x.Password).IsModified = true;
            _context.Entry(user).Property(x => x.PasswordResetToken).IsModified = true;
            _context.Entry(user).Property(x => x.PasswordResetTokenExpiry).IsModified = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<(bool Success, string Message)> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
                return (false, "Kullanıcı bulunamadı.");

            // Mevcut şifreyi kontrol et
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Password))
                return (false, "Mevcut şifre yanlış.");

            // Yeni şifreyi kaydet
            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync();
            return (true, "Şifre başarıyla değiştirildi.");
        }
    }
}
