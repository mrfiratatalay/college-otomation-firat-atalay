using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public class UserAuthorizationService : BaseAuthorizationService
    {
        public UserAuthorizationService(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> CanManageUsers(string userRole)
        {
            if (await IsAdmin(userRole))
                return true;

            throw new UnauthorizedBusinessException("Kullanıcı yönetimi için yönetici yetkisi gerekiyor.");
        }

        public async Task<bool> CanUpdateUser(int requestingUserId, int targetUserId, string userRole)
        {
            // Admins can update any user
            if (await IsAdmin(userRole))
                return true;

            // Users can only update their own profiles
            if (requestingUserId != targetUserId)
                throw new UnauthorizedBusinessException("Sadece kendi profilinizi güncelleyebilirsiniz.");

            return true;
        }

        public async Task<bool> CanViewUserDetails(int requestingUserId, int targetUserId, string userRole)
        {
            // Admins can view any user's details
            if (await IsAdmin(userRole))
                return true;

            // Advisors can view details of students in their clubs
            if (await IsAdvisor(userRole))
            {
                var hasStudentInClubs = await _context.ClubMembership
                    .Include(cm => cm.Club)
                    .Include(cm => cm.Club.Advisor)
                    .AnyAsync(cm => 
                        cm.UserId == targetUserId && 
                        cm.Club.AdvisorId == requestingUserId);

                if (hasStudentInClubs)
                    return true;
            }

            // Club presidents can view details of their club members
            var isPresidentOfUsersClub = await _context.ClubMembership
                .AnyAsync(cm => 
                    cm.UserId == requestingUserId && 
                    cm.Role == "başkan" && 
                    cm.Status == "Onaylı" &&
                    _context.ClubMembership.Any(m => 
                        m.UserId == targetUserId && 
                        m.ClubId == cm.ClubId));

            if (isPresidentOfUsersClub)
                return true;

            // Users can view their own details
            if (requestingUserId == targetUserId)
                return true;

            throw new UnauthorizedBusinessException("Bu kullanıcının detaylarını görüntüleme yetkiniz yok.");
        }
    }
}