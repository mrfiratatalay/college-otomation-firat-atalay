using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public class AdvisorAuthorizationService : BaseAuthorizationService
    {
        public AdvisorAuthorizationService(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> CanManageAdvisors(string userRole)
        {
            if (await IsAdmin(userRole))
                return true;

            throw new UnauthorizedBusinessException("Danışman yönetimi için yönetici yetkisi gerekiyor.");
        }

        public async Task<bool> CanAssignAdvisor(string userRole, int clubId)
        {
            if (await IsAdmin(userRole))
                return true;

            // Kulüp aktif mi kontrol et
            await ValidateClubStatus(clubId);

            throw new UnauthorizedBusinessException("Danışman atama yetkisi yok.");
        }

        public async Task<bool> CanAccessAdvisorPanel(int userId, string userRole)
        {
            if (await IsAdmin(userRole))
                return true;

            if (!await IsAdvisor(userRole))
                throw new UnauthorizedBusinessException("Danışman paneline erişim yetkiniz yok.");

            var isAssignedToAnyClub = await _context.ClubMembership
                .AnyAsync(m => m.UserId == userId && m.Role == "advisor" && m.Status == "approved");

            if (!isAssignedToAnyClub)
                throw new UnauthorizedBusinessException("Herhangi bir kulübe danışman olarak atanmamışsınız.");

            return true;
        }

        public async Task<bool> CanViewClubDetails(int advisorId, int clubId)
        {
            var isAdvisorOfClub = await _context.ClubMembership
                .AnyAsync(m => m.UserId == advisorId && m.ClubId == clubId && m.Role == "advisor" && m.Status == "approved");

            if (!isAdvisorOfClub)
                throw new UnauthorizedBusinessException("Bu topluluğun danışmanı değilsiniz.");

            return true;
        }

        public async Task<bool> CanApproveClubActivities(int advisorId, int clubId)
        {
            await CanViewClubDetails(advisorId, clubId);

            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            if (club.Status.ToLower() != "active")
                throw new BusinessException("İnaktif toplulukların etkinliklerini onaylayamazsınız.");

            return true;
        }
    }
}
