using OtpNet;
using System.Security.Cryptography;

namespace ElasoftCommunityManagementSystem.Services
{
    public interface ITwoFactorService
    {
        string GenerateSecretKey();
        string GenerateQrCodeUrl(string email, string secretKey);
        bool ValidateCode(string secretKey, string code);
    }

    public class TwoFactorService : ITwoFactorService
    {
        private const string Issuer = "ElasoftCMS";

        public string GenerateSecretKey()
        {
            var secretKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretKey);
            }
            return Base32Encoding.ToString(secretKey);
        }

        public string GenerateQrCodeUrl(string email, string secretKey)
        {
            var encodedIssuer = Uri.EscapeDataString(Issuer);
            var encodedEmail = Uri.EscapeDataString(email);
            return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}";
        }

        public bool ValidateCode(string secretKey, string code)
        {
            try
            {
                var totp = new Totp(Base32Encoding.ToBytes(secretKey));
                return totp.VerifyTotp(code, out _, VerificationWindow.RfcSpecifiedNetworkDelay);
            }
            catch
            {
                return false;
            }
        }
    }
}