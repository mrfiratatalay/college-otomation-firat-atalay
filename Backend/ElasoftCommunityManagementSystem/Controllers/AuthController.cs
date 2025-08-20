using System.Security.Claims;
using ElasoftCommunityManagementSystem.Dtos.AuthDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            // Log the request to troubleshoot
            Console.WriteLine(
                $"Register request received: {System.Text.Json.JsonSerializer.Serialize(model)}"
            );

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new
                    {
                        success = false,
                        message = "Geçersiz form verisi.",
                        errors,
                    }
                );
            }

            try
            {
                var result = await _authService.Register(model);
                var (success, message, tempPassword) = result;

                if (!success)
                    return BadRequest(new { success, message });

                return Ok(
                    new
                    {
                        success = true,
                        message = "Kayıt başarılı! E-posta gönderimi başarısız olabilir, geçici şifreniz:",
                        temporaryPassword = tempPassword,
                    }
                );
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Registration error: {ex.Message}");
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Kayıt sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    }
                );
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            // Log the request to troubleshoot
            Console.WriteLine(
                $"Login request received: {System.Text.Json.JsonSerializer.Serialize(request)}"
            );

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(
                    new
                    {
                        success = false,
                        message = "Geçersiz form verisi.",
                        errors,
                    }
                );
            }

            try
            {
                var loginResult = await _authService.Login(request.Email, request.Password);
                if (loginResult == null)
                    return Unauthorized(
                        new { success = false, message = "E-posta veya şifre hatalı." }
                    );

                return Ok(
                    new
                    {
                        success = true,
                        message = "Giriş başarılı",
                        token = loginResult.AccessToken,
                        refreshToken = loginResult.RefreshToken,
                        user = new
                        {
                            id = loginResult.User.UserId,
                            email = loginResult.User.Email,
                            name = loginResult.User.Name,
                            surname = loginResult.User.Surname,
                            userType = loginResult.User.Role,
                        },
                    }
                );
            }
            catch (BusinessException ex)
            {
                // Log the exception
                Console.WriteLine($"Login error: {ex.Message}");
                return Unauthorized(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Unexpected login error: {ex.Message}");
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Giriş sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin.",
                    }
                );
            }
        }

        [HttpGet("whoami")]
        [Authorize]
        public IActionResult WhoAmI()
        {
            var userId = User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;
            var userEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

            return Ok(
                new
                {
                    UserId = userId,
                    Email = userEmail,
                    Role = userRole,
                }
            );
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var result = await _authService.RefreshToken(refreshToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Invalid refresh token" });
            }
        }

        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] string refreshToken)
        {
            await _authService.RevokeRefreshToken(refreshToken);
            return Ok(new { message = "Token revoked successfully" });
        }

        [HttpPost("2fa/enable")]
        [Authorize]
        public async Task<IActionResult> Enable2FA()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            try
            {
                var (qrCodeUrl, secretKey) = await _authService.Enable2FA(userId);
                return Ok(new { qrCodeUrl, manualCode = secretKey });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "2FA etkinleştirme hatası: " + ex.Message });
            }
        }

        [HttpPost("2fa/verify")]
        [Authorize]
        public async Task<IActionResult> Verify2FA([FromBody] string code)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            try
            {
                var isValid = await _authService.Verify2FA(userId, code);
                if (!isValid)
                    return BadRequest(new { message = "Geçersiz doğrulama kodu." });
                return Ok(new { message = "2FA başarıyla etkinleştirildi." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "2FA doğrulama hatası: " + ex.Message });
            }
        }

        [HttpPost("2fa/disable")]
        [Authorize]
        public async Task<IActionResult> Disable2FA()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            try
            {
                await _authService.Disable2FA(userId);
                return Ok(new { message = "2FA devre dışı bırakıldı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "2FA devre dışı bırakma hatası: " + ex.Message });
            }
        }

        [HttpPost("2fa/validate")]
        public async Task<IActionResult> Validate2FA([FromBody] Validate2FADto dto)
        {
            try
            {
                var isValid = await _authService.Validate2FA(dto.UserId, dto.Code);
                if (!isValid)
                    return BadRequest(new { message = "Geçersiz 2FA kodu." });
                return Ok(new { message = "2FA doğrulaması başarılı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "2FA doğrulama hatası: " + ex.Message });
            }
        }

        [HttpPost("password-reset/request")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] string email)
        {
            try
            {
                await _authService.RequestPasswordReset(email);
                return Ok(
                    new { message = "Şifre sıfırlama talimatları e-posta adresinize gönderildi." }
                );
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Şifre sıfırlama isteği hatası: " + ex.Message });
            }
        }

        [HttpPost("password-reset/reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var success = await _authService.ResetPassword(
                    dto.Email,
                    dto.Token,
                    dto.NewPassword
                );
                if (!success)
                    return BadRequest(new { message = "Geçersiz veya süresi dolmuş token." });
                return Ok(new { message = "Şifreniz başarıyla sıfırlandı." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Şifre sıfırlama hatası: " + ex.Message });
            }
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _authService.ChangePassword(userId, dto.CurrentPassword, dto.NewPassword);
            if (!result.Success)
                return BadRequest(new { success = false, message = result.Message });
            return Ok(new { success = true, message = "Şifre başarıyla değiştirildi." });
        }
    }
}
