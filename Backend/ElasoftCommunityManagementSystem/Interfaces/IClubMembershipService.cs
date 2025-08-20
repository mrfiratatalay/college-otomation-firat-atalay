using ElasoftCommunityManagementSystem.Dtos.ClubMembershipoDtos;
using ElasoftCommunityManagementSystem.Models;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IClubMembershipService
    {
        Task ApplyToClubAsync(int userId, ClubMembershipDto dto);
        Task<List<object>> GetMyClubApplicationsAsync(int userId);
        Task ApproveMembershipAsync(int membershipId, int userId, string userRole);
        Task RejectMembershipAsync(int membershipId, int userId, string userRole);
        Task<object> GetUserRolesInClubsAsync(int userId);
        Task SetClubLeaderAsync(int membershipId, int userId);
        Task<List<object>> GetPendingApplicationsForAdvisorAsync(int advisorId);
        Task<List<object>> GetPendingApplicationsForMyLedClubsAsync(int userId);
        Task<List<ClubMemberListDto>> GetClubMembersAsync(int clubId);
        Task<List<object>> GetApprovedMembersAsync(int clubId);
        Task DeleteApprovedMemberAsync(int membershipId, int userId);
        Task<object> GetClubMemberUserDetailsAsync(int userId);
        Task LeaveClubAsync(int membershipId, int userId);
        Task<bool> ValidateClubPresidentUniquenessAsync(int clubId);
        Task<ClubMembershipModel?> GetClubPresidentAsync(int clubId);
        Task ChangeClubPresidentAsync(int clubId, int newPresidentUserId, int adminUserId, string userRole);
    }
}
