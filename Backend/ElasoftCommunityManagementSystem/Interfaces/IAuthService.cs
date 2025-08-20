using ElasoftCommunityManagementSystem.Services;
using ElasoftCommunityManagementSystem.Dtos.AuthDtos;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, string TempPassword)> Register(RegisterDto dto);
        Task<LoginResult?> Login(string email, string password);
        Task<LoginResult> RefreshToken(string refreshToken);
        Task RevokeRefreshToken(string refreshToken);
        
        Task<(string qrCodeUrl, string secretKey)> Enable2FA(int userId);
        Task<bool> Verify2FA(int userId, string code);
        Task Disable2FA(int userId);
        Task<bool> Validate2FA(int userId, string code);
        
        Task RequestPasswordReset(string email);
        Task<bool> ResetPassword(string email, string token, string newPassword);
        Task<(bool Success, string Message)> ChangePassword(int userId, string currentPassword, string newPassword);
    }
}
