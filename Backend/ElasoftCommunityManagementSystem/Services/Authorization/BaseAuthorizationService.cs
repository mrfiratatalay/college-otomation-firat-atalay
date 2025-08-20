using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public abstract class BaseAuthorizationService
    {
        protected readonly AppDbContext _context;

        protected BaseAuthorizationService(AppDbContext context)
        {
            _context = context;
        }

        protected async Task<bool> IsAdmin(string userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole))
                throw new ValidationException("Kullanıcı rolü belirtilmemiş.");

            return userRole.ToLower() == "admin";
        }

        protected async Task<bool> IsAdvisor(string userRole)
        {
            if (string.IsNullOrWhiteSpace(userRole))
                throw new ValidationException("Kullanıcı rolü belirtilmemiş.");

            return userRole.ToLower() == "danışman";
        }

        protected async Task<bool> IsClubPresident(int userId, int clubId)
        {
            return await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId
                && m.ClubId == clubId
                && EF.Functions.Like(m.Role.ToLower(), "başkan")
                && m.Status.ToLower() == "approved"
            );
        }

        protected async Task<bool> IsClubMember(int userId, int clubId)
        {
            return await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId && m.ClubId == clubId && m.Status.ToLower() == "approved"
            );
        }

        protected async Task ValidateClubStatus(int clubId)
        {
            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            if (club.Status.ToLower() != "active")
                throw new BusinessException("Bu topluluk aktif değil.");
        }
    }
}
