using ElasoftCommunityManagementSystem.Dtos.UserDtos;
using ElasoftCommunityManagementSystem.Models;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IUserService
    {
        Task<UserModel?> GetUserByIdAsync(int userId);
        Task<List<UserModel>> GetAllUsersAsync();
        Task<bool> UpdateUserAsync(UserModel user);
        Task<bool> DeleteUserAsync(int userId);
        Task<bool> ChangeUserRoleAsync(int userId, string role);
        Task<bool> UpdateUserProfileAsync(int userId, string name, string surname, string phoneNumber);
        Task<List<ClubDepartmentDistributionDto>> GetClubDepartmentDistributionAsync(int? clubId);
    }
} 