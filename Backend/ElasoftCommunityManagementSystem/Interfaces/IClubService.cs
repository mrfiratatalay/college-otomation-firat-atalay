using ElasoftCommunityManagementSystem.Dtos.ClubDtos;
using Microsoft.Identity.Client;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IClubService
    {
        Task<List<object>> GetClubsAsync(string? status);
        Task<object> GetClubByIdAsync(int clubId, int userId, string userRole);
        Task<object> CreateClubAsync(CreateClubDto dto, string userRole);
        Task<int> ClubMemberCountAsync(int clubId, int userId, string userRole );
        Task<int> ClubEventCountAsync(int clubId, int userId, string userRole);
        Task ApproveClubAsync(int clubId);
        Task DeleteClubAsync(int clubId);
        Task<object> UpdateClubAsync(int clubId, CreateClubDto dto, int userId, string userRole);
        Task<List<object>> GetClubsByAdvisorAsync(int advisorId);
        Task<List<object>> GetClubMemberStatsAsync();

    }
}
