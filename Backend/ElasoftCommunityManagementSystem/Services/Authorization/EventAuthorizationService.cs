using System;
using System.Linq;
using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services.Authorization
{
    public class EventAuthorizationService : BaseAuthorizationService
    {
        public EventAuthorizationService(AppDbContext context)
            : base(context) { }

        public async Task<bool> CanCreateEvent(int userId, string userRole, int clubId)
        {
            // Admin ve danışmanlar etkinlik oluşturabilir
            if (await IsAdmin(userRole) || await IsAdvisor(userRole))
                return true;

            // Topluluk durumu kontrol edilsin
            await ValidateClubStatus(clubId);

            // Kullanıcının kulüp başkanı olup olmadığını kontrol et
            if (!await IsClubPresident(userId, clubId))
                throw new UnauthorizedBusinessException("Etkinlik oluşturma yetkiniz yok. Sadece topluluk başkanları etkinlik oluşturabilir.");

            return true;
        }

        public async Task<bool> CanUpdateEvent(int userId, string userRole, int eventId)
        {
            var @event = await _context
                .Event.Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (@event == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            // Geçmişte başlamış etkinlikler güncellenemez
            if (@event.StartDate < DateTime.Now)
                throw new BusinessException("Geçmiş tarihli etkinlikler güncellenemez.");

            // Admin ve danışmanlar güncelleyebilir
            if (await IsAdmin(userRole) || await IsAdvisor(userRole))
                return true;

            // Kullanıcı kulüp başkanı değilse hata fırlat
            if (!await IsClubPresident(userId, @event.ClubId))
                throw new UnauthorizedBusinessException("Etkinlik güncelleme yetkiniz yok.");

            return true;
        }

        public async Task<bool> CanDeleteEvent(int userId, string userRole, int eventId)
        {
            var @event = await _context
                .Event.Include(e => e.Club)
                .Include(e => e.EventParticipants)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (@event == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            // Geçmişte başlayan etkinlikler silinemez
            if (@event.StartDate < DateTime.Now)
                throw new BusinessException("Geçmiş tarihli etkinlikler silinemez.");

            // Katılımcısı olan etkinlik silinemez
            if (@event.EventParticipants.Any())
                throw new BusinessException("Katılımcısı olan etkinlik silinemez.");

            // Admin ve danışmanlar silebilir
            if (await IsAdmin(userRole) || await IsAdvisor(userRole))
                return true;

            // Kullanıcı kulüp başkanı değilse hata fırlat
            if (!await IsClubPresident(userId, @event.ClubId))
                throw new UnauthorizedBusinessException("Etkinlik silme yetkiniz yok.");

            return true;
        }

        public async Task<bool> IsUserAuthorizedForEvent(int userId, string userRole, int paramId)
        {
            // Admin ve danışmanlar her türlü işlemi yapabilir
            if (await IsAdmin(userRole) || await IsAdvisor(userRole))
                return true;

            // Parametre bir clubId mi yoksa eventId mi belirle
            bool isEventId = await _context.Event.AnyAsync(e => e.EventId == paramId);
            int clubId;

            if (isEventId)
            {
                // paramId bir eventId, ilgili clubId'yi bul
                var eventEntity = await _context.Event.FindAsync(paramId);
                if (eventEntity == null)
                    throw new ResourceNotFoundException("Etkinlik bulunamadı.");

                clubId = eventEntity.ClubId;
            }
            else
            {
                // paramId doğrudan bir clubId
                clubId = paramId;
            }

            // Topluluk durumu kontrol edilsin
            await ValidateClubStatus(clubId);

            // Kullanıcının kulübe üye olup olmadığını kontrol et
            if (!await IsClubMember(userId, clubId))
                throw new UnauthorizedBusinessException("Bu işlem için kulüp üyesi olmanız gerekiyor.");

            // Kullanıcı başkan ise veya normal üye ise ve işlem etkinliğe katılma/ayrılma ise
            if (await IsClubPresident(userId, clubId) || userRole.ToLower() == "member")
                return true;

            throw new UnauthorizedBusinessException("Bu işlem için yetkiniz bulunmuyor.");
        }

        public async Task<bool> CanJoinEvent(int userId, int eventId)
        {
            var @event = await _context
                .Event.Include(e => e.Club)
                .Include(e => e.EventParticipants)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (@event == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            // Topluluk durumu kontrol edilsin
            await ValidateClubStatus(@event.ClubId);

            // Etkinlik geçmişte başlamışsa katılım yapılamaz
            if (@event.StartDate < DateTime.Now)
                throw new BusinessException("Geçmiş tarihli etkinliğe katılım yapılamaz.");

            // Kullanıcı zaten katılıyorsa hata fırlat
            if (@event.EventParticipants.Any(p => p.UserId == userId))
                throw new BusinessException("Bu etkinliğe zaten katılıyorsunuz.");

            // Etkinlik dolu mu kontrol et
            if (@event.ParticipantCount >= @event.MaxParticipants)
                throw new BusinessException("Etkinlik katılımcı sayısı dolmuştur.");

            // Kullanıcı kulüp üyesi değilse hata fırlat
            if (!await IsClubMember(userId, @event.ClubId))
                throw new UnauthorizedBusinessException("Bu etkinliğe katılmak için önce kulübe üye olmanız gerekiyor.");

            return true;
        }

        public async Task<bool> CanLeaveEvent(int userId, int eventId)
        {
            var @event = await _context
                .Event.Include(e => e.EventParticipants)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (@event == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            // Geçmiş tarihli etkinlikten ayrılma yapılamaz
            if (@event.StartDate < DateTime.Now)
                throw new BusinessException("Geçmiş tarihli etkinlikten ayrılamazsınız.");

            var participant = @event.EventParticipants.FirstOrDefault(p => p.UserId == userId);
            if (participant == null)
                throw new ResourceNotFoundException("Bu etkinliğe katılım kaydınız bulunmuyor.");

            return true;
        }
    }
}
