using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public class ClubAuthorizationService : BaseAuthorizationService
    {
        public ClubAuthorizationService(AppDbContext context)
            : base(context) { }

        public async Task<bool> CanManageClub(int userId, string userRole, int clubId)
        {
            // Admin → her şeye erişir
            if (userRole == "admin")
                return true;

            // Advisor → sadece sorumlu olduğu kulübe erişir
            if (userRole == "advisor")
            {
                var isAdvisorOfClub = await _context.ClubMembership.AnyAsync(m =>
                    m.UserId == userId && m.ClubId == clubId && m.Role.ToLower() == "advisor"
                );

                if (isAdvisorOfClub)
                    return true;

                throw new UnauthorizedBusinessException("Bu kulübe erişim yetkiniz yok.");
            }

            // Başkan → sadece başkan olduğu kulübe erişir
            var isPresident = await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId && m.ClubId == clubId && m.Role.ToLower() == "başkan"
            );

            if (!isPresident)
                throw new UnauthorizedBusinessException("Bu kulübe erişim yetkiniz yok.");

            return true;
        }

        public async Task<bool> CanJoinClub(int userId, int clubId)
        {
            // Check if club is active
            await ValidateClubStatus(clubId);

            // Sadece aynı kulüpte başkan ve üye olma kontrolü yapılıyor
            // Diğer kulüplerde başkan olup olmama kontrolü kaldırıldı
            if (await IsClubMember(userId, clubId))
                throw new BusinessException("Bu kulübe zaten üyesiniz.");

            return true;
        }

        public async Task CanApproveMembers(int userId, string userRole, int clubId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new UnauthorizedBusinessException("Kullanıcı bulunamadı.");

            // Admin ise her kulübe onay/reddetme yetkisi var
            if (user.Role.ToLower() == "admin")
                return;

            // Danışman ise, kulüpte danışman olarak atanmış mı kontrol et
            if (user.Role.ToLower() == "advisor")
            {
                var isAdvisor = await _context.ClubMembership.AnyAsync(m =>
                    m.UserId == userId
                    && m.ClubId == clubId
                    && m.Role.ToLower() == "advisor"
                    && m.Status == "approved"
                );

                if (isAdvisor)
                    return;
            }

            // Başkan ise, kulübün onaylı başkanı mı kontrol et
            var isPresident = await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId
                && m.ClubId == clubId
                && m.Role.ToLower() == "başkan"
                && m.Status == "approved"
            );

            if (isPresident)
                return;

            throw new UnauthorizedBusinessException("Bu işlem için yetkiniz bulunmuyor.");
        }

        public async Task<bool> CanDeleteClub(string userRole, int clubId)
        {
            // Only admin can delete clubs
            if (!await IsAdmin(userRole))
                throw new UnauthorizedBusinessException("Topluluk silme yetkiniz yok.");

            // Verify club exists
            var club = await _context.Club.FindAsync(clubId);
            if (club == null)
                throw new ResourceNotFoundException("Topluluk bulunamadı.");

            return true;
        }

        // Kullanıcının bir kulübün başkanı olup olmadığını kontrol eden metod
        public async Task<bool> IsUserClubLeaderAnywhere(int userId)
        {
            return await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId
                && EF.Functions.Like(m.Role.ToLower(), "başkan")
                && (m.Status.ToLower() == "approved" || m.Status.ToLower() == "onaylı")
            );
        }

        // Bir kullanıcının belirli bir kulübün başkanı olup olmadığını kontrol eden metod
        public async Task<bool> IsUserLeaderOfClub(int userId, int clubId)
        {
            return await _context.ClubMembership.AnyAsync(m =>
                m.UserId == userId
                && m.ClubId == clubId
                && EF.Functions.Like(m.Role, "başkan")
                && m.Status == "Onaylı"
            );
        }
    }
}
