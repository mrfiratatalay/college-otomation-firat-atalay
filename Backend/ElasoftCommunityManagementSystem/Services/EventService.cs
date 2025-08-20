using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Dtos.EventDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Services.Authorization;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class EventService : IEventService
    {
        private readonly AppDbContext _context;
        private readonly Authorization.EventAuthorizationService _authService;

        public EventService(
            AppDbContext context,
            Authorization.EventAuthorizationService authService
        )
        {
            _context = context;
            _authService = authService;
        }

        public async Task<List<EventDto>> GetEvents(
            int? clubId,
            int userId,
            string userRole,
            string? search
        )
        {
            var query = _context
                .Event.Include(e => e.Club)
                .Include(e => e.EventParticipants)
                .ThenInclude(p => p.User)
                .AsQueryable();

            if (clubId.HasValue)
                query = query.Where(e => e.ClubId == clubId.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(e => e.Name.Contains(search));

            var events = await query.ToListAsync();
            var result = new List<EventDto>();
            var now = DateTime.Now;

            foreach (var ev in events)
            {
                var activeParticipantCount = ev.EventParticipants.Count();

                var status =
                    activeParticipantCount >= ev.MaxParticipants ? "Dolu"
                    : now < ev.StartDate && (ev.StartDate - now).TotalDays <= 7 ? "Yaklaşan"
                    : now >= ev.StartDate && now <= ev.EndDate ? "Devam Eden"
                    : now > ev.EndDate ? "Tamamlandı"
                    : "Planlanan";

                var isParticipating = await CheckUserParticipation(userId, ev.EventId);

                var dto = new EventDto
                {
                    EventId = ev.EventId,
                    Name = ev.Name,
                    Description = ev.Description,
                    StartDate = ev.StartDate,
                    EndDate = ev.EndDate,
                    ClubName = ev.Club.Name,
                    MaxParticipants = ev.MaxParticipants,
                    ParticipantCount = activeParticipantCount,
                    ImageUrl = ev.ImageUrl,
                    Status = ev.Status,
                    IsParticipating = isParticipating,
                    Image = ev.Image != null ? Convert.ToBase64String(ev.Image) : null,
                    Category = ev.Category,
                    RejectReason = ev.RejectReason,
                    ApplicationFormDocumentId = ev.ApplicationFormDocumentId,
                    ManagementBoardFormDocumentId = ev.ManagementBoardFormDocumentId,
                    ResultReportDocumentId = ev.ResultReportDocumentId,
                };

                var isPresident = await _context.ClubMembership.AnyAsync(m =>
                    m.UserId == userId
                    && m.ClubId == ev.ClubId
                    && m.Role.ToLower() == "leader"
                    && m.Status.ToLower() == "onaylı"
                );

                if (userRole.ToLower() == "admin" || userRole.ToLower() == "advisor" || isPresident)
                {
                    dto.Participants = ev
                        .EventParticipants.Select(p => $"{p.User.Name} {p.User.Surname}")
                        .ToList();
                }

                result.Add(dto);
            }

            return result;
        }

        public async Task<EventDto> CreateEvent(CreateEventDto dto, int userId, string userRole)
        {
            if (dto == null)
                throw new ValidationException("Etkinlik verisi boş olamaz.");

            var isAuthorized = await _authService.CanCreateEvent(userId, userRole, dto.ClubId);
            if (!isAuthorized)
                throw new UnauthorizedBusinessException(
                    "Bu topluluk için etkinlik oluşturma yetkiniz yok."
                );

            if (dto.StartDate < DateTime.Now || dto.EndDate <= dto.StartDate)
                throw new ValidationException("Etkinlik tarihleri geçersiz.");

            byte[]? imageData = null;
            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.ImageFile.CopyToAsync(ms);
                imageData = ms.ToArray();
            }

            string? applicationFormDocumentId = null;
            string? managementBoardFormDocumentId = null;
            if (dto.ApplicationFormFile != null && dto.ApplicationFormFile.Length > 0)
            {
                var ext = Path.GetExtension(dto.ApplicationFormFile.FileName);
                var fileName = $"application_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine("wwwroot", "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ApplicationFormFile.CopyToAsync(stream);
                }
                applicationFormDocumentId = fileName;
            }
            if (dto.ManagementBoardFormFile != null && dto.ManagementBoardFormFile.Length > 0)
            {
                var ext = Path.GetExtension(dto.ManagementBoardFormFile.FileName);
                var fileName = $"management_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine("wwwroot", "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ManagementBoardFormFile.CopyToAsync(stream);
                }
                managementBoardFormDocumentId = fileName;
            }

            var newEvent = new EventModel
            {
                ClubId = dto.ClubId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedAt = DateTime.UtcNow,
                MaxParticipants = dto.MaxParticipants,
                ImageUrl = dto.ImageUrl,
                Image = imageData,
                Status = "advisor_pending",
                ParticipantCount = 0,
                Category = dto.Category,
                ApplicationFormDocumentId = applicationFormDocumentId,
                ManagementBoardFormDocumentId = managementBoardFormDocumentId,
            };

            _context.Event.Add(newEvent);
            await _context.SaveChangesAsync();

            return new EventDto
            {
                EventId = newEvent.EventId,
                Name = newEvent.Name,
                Description = newEvent.Description,
                StartDate = newEvent.StartDate,
                EndDate = newEvent.EndDate,
                ClubId = newEvent.ClubId,
                MaxParticipants = newEvent.MaxParticipants,
                ParticipantCount = 0,
                ImageUrl = newEvent.ImageUrl,
                Image = newEvent.Image != null ? Convert.ToBase64String(newEvent.Image) : null,
                Status = newEvent.Status,
                Success = true,
                Message = "Etkinlik başarıyla oluşturuldu ve danışman onayı bekleniyor.",
                Category = newEvent.Category,
                ApplicationFormDocumentId = newEvent.ApplicationFormDocumentId,
                ManagementBoardFormDocumentId = newEvent.ManagementBoardFormDocumentId,
                ResultReportDocumentId = newEvent.ResultReportDocumentId,
            };
        }

        public async Task<EventDto> UpdateEvent(
            int id,
            CreateEventDto dto,
            int userId,
            string userRole
        )
        {
            await _authService.IsUserAuthorizedForEvent(userId, userRole, dto.ClubId);

            var eventEntity = await _context.Event.FindAsync(id);
            if (eventEntity == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            if (eventEntity.StartDate < DateTime.Now)
                throw new ValidationException("Geçmiş tarihli etkinlikler güncellenemez.");

            if (!string.IsNullOrEmpty(dto.ImageUrl))
                eventEntity.ImageUrl = dto.ImageUrl;

            if (dto.ImageFile != null && dto.ImageFile.Length > 0)
            {
                using var ms = new MemoryStream();
                await dto.ImageFile.CopyToAsync(ms);
                eventEntity.Image = ms.ToArray();
            }

            eventEntity.Name = dto.Name;
            eventEntity.Description = dto.Description;
            eventEntity.StartDate = dto.StartDate;
            eventEntity.EndDate = dto.EndDate;
            eventEntity.MaxParticipants = dto.MaxParticipants;
            eventEntity.Category = dto.Category;

            await _context.SaveChangesAsync();

            return new EventDto
            {
                EventId = eventEntity.EventId,
                Name = eventEntity.Name,
                Description = eventEntity.Description,
                StartDate = eventEntity.StartDate,
                EndDate = eventEntity.EndDate,
                ClubId = eventEntity.ClubId,
                MaxParticipants = eventEntity.MaxParticipants,
                ParticipantCount = eventEntity.ParticipantCount,
                ImageUrl = eventEntity.ImageUrl,
                Image =
                    eventEntity.Image != null ? Convert.ToBase64String(eventEntity.Image) : null,
                Success = true,
                Message = "Etkinlik başarıyla güncellendi.",
                Category = eventEntity.Category,
                ResultReportDocumentId = eventEntity.ResultReportDocumentId,
            };
        }

        public async Task<EventDto> DeleteEvent(int id, int userId, string userRole)
        {
            await _authService.IsUserAuthorizedForEvent(userId, userRole, id);

            var existingEvent = await _context
                .Event.Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.EventId == id);
            if (existingEvent == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            if (existingEvent.StartDate < DateTime.Now)
                throw new ValidationException("Geçmiş tarihli etkinlikler silinemez.");

            _context.Event.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return new EventDto
            {
                EventId = id,
                Success = true,
                Message = "Etkinlik başarıyla silindi.",
            };
        }

        public async Task<EventDto> JoinEvent(int eventId, int userId)
        {
            await _authService.CanJoinEvent(userId, eventId);

            var eventEntity = await _context
                .Event.Include(e => e.EventParticipants)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventEntity == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            var participant = new EventParticipantModel
            {
                EventId = eventId,
                UserId = userId,
                RegisteredAt = DateTime.UtcNow,
            };

            eventEntity.EventParticipants.Add(participant);
            await _context.SaveChangesAsync();

            var activeParticipantCount = eventEntity.EventParticipants.Count();

            return new EventDto
            {
                EventId = eventEntity.EventId,
                Success = true,
                Message = "Etkinliğe başarıyla katıldınız.",
                IsParticipating = true,
                ParticipantCount = activeParticipantCount,
            };
        }

        public async Task<EventDto> LeaveEvent(int eventId, int userId)
        {
            await _authService.CanLeaveEvent(userId, eventId);

            var eventEntity = await _context
                .Event.Include(e => e.EventParticipants)
                .FirstOrDefaultAsync(e => e.EventId == eventId);

            if (eventEntity == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            var participant = eventEntity.EventParticipants.FirstOrDefault(p =>
                p.UserId == userId
            );
            if (participant == null)
                throw new ResourceNotFoundException("Bu etkinliğe katılım kaydınız bulunmuyor.");

            eventEntity.EventParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            var activeParticipantCount = eventEntity.EventParticipants.Count();

            return new EventDto
            {
                EventId = eventEntity.EventId,
                Success = true,
                Message = "Etkinlikten başarıyla ayrıldınız.",
                IsParticipating = false,
                ParticipantCount = activeParticipantCount,
            };
        }

        public async Task<List<EventDto>> GetEventsForAuthorizedUser(int userId, string userRole)
        {
            var validRoles = new[] { "advisor", "leader" };
            if (!validRoles.Contains(userRole.ToLower()))
                throw new UnauthorizedBusinessException(
                    "Bu işlem yalnızca advisor veya leader kullanıcılar içindir."
                );

            var clubIds = await _context
                .ClubMembership.Where(m =>
                    m.UserId == userId
                    && (m.Role.ToLower() == "advisor" || m.Role.ToLower() == "leader")
                    && m.Status.ToLower() == "approved"
                )
                .Select(m => m.ClubId)
                .Distinct()
                .ToListAsync();

            if (!clubIds.Any())
                return new List<EventDto>();

            var events = await _context
                .Event.Include(e => e.Club)
                .Include(e => e.EventParticipants)
                .ThenInclude(p => p.User)
                .Where(e => clubIds.Contains(e.ClubId))
                .OrderByDescending(e => e.StartDate)
                .ToListAsync();

            var now = DateTime.Now;
            var result = new List<EventDto>();

            foreach (var ev in events)
            {
                var status =
                    ev.ParticipantCount >= ev.MaxParticipants ? "Dolu"
                    : now < ev.StartDate && (ev.StartDate - now).TotalDays <= 7 ? "Yaklaşan"
                    : now >= ev.StartDate && now <= ev.EndDate ? "Devam Eden"
                    : now > ev.EndDate ? "Tamamlandı"
                    : "Planlanan";

                result.Add(
                    new EventDto
                    {
                        EventId = ev.EventId,
                        Name = ev.Name,
                        Description = ev.Description,
                        StartDate = ev.StartDate,
                        EndDate = ev.EndDate,
                        ClubId = ev.ClubId,
                        ClubName = ev.Club.Name,
                        MaxParticipants = ev.MaxParticipants,
                        ParticipantCount = ev.ParticipantCount,
                        ImageUrl = ev.ImageUrl,
                        Status = ev.Status,
                        Image = ev.Image != null ? Convert.ToBase64String(ev.Image) : null,
                        Participants = ev
                            .EventParticipants.Select(p => $"{p.User.Name} {p.User.Surname}")
                            .ToList(),
                        Category = ev.Category,
                        ResultReportDocumentId = ev.ResultReportDocumentId,
                        ResultReportStatus = !string.IsNullOrEmpty(ev.ResultReportDocumentId)
                            ? "yüklendi"
                            : (now > ev.EndDate.AddDays(15) ? "süre dolmuş" : "yüklenmedi"),
                    }
                );
            }

            return result;
        }

        public async Task ApproveOrRejectEvent(
            int eventId,
            int userId,
            string status,
            string userRole,
            string? rejectReason = null
        )
        {
            var eventEntity = await _context
                .Event.Include(e => e.Club)
                .FirstOrDefaultAsync(e => e.EventId == eventId);
            if (eventEntity == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            if (userRole == "advisor")
            {
                if (eventEntity.Status != "advisor_pending")
                    throw new BusinessException("Etkinlik danışman onayında değil.");

                var isAdvisor = await _context.ClubMembership.AnyAsync(cm =>
                    cm.UserId == userId
                    && cm.ClubId == eventEntity.ClubId
                    && cm.Role.ToLower() == "advisor"
                    && cm.Status.ToLower() == "approved"
                );
                if (!isAdvisor)
                    throw new UnauthorizedBusinessException("Bu etkinliği onaylama yetkiniz yok.");

                if (status == "approved")
                {
                    eventEntity.Status = "admin_pending";
                    eventEntity.RejectReason = null;
                }
                else if (status == "rejected")
                {
                    eventEntity.Status = "rejected";
                    eventEntity.RejectReason = null;
                }
                else
                    throw new ValidationException("Geçersiz durum.");
            }
            else if (userRole == "admin")
            {
                if (eventEntity.Status != "admin_pending")
                    throw new BusinessException("Etkinlik admin onayında değil.");

                if (status == "approved")
                {
                    eventEntity.Status = "approved";
                    eventEntity.RejectReason = null;
                }
                else if (status == "rejected")
                {
                    eventEntity.Status = "rejected";
                    eventEntity.RejectReason = rejectReason;
                }
                else
                    throw new ValidationException("Geçersiz durum.");
            }
            else
            {
                throw new UnauthorizedBusinessException(
                    "Sadece danışman veya admin onay/reddedebilir."
                );
            }

            eventEntity.UpdatedAt = DateTime.UtcNow;

            // RED durumunda rezervasyonları temizle
            if (status == "rejected")
            {
                var reservations = await _context.EventReservations
                    .Where(r => r.EventId == eventId)
                    .ToListAsync();

                if (reservations.Any())
                {
                    _context.EventReservations.RemoveRange(reservations);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<EventDto>> GetCompletedEvents(
            int clubId,
            int userId,
            string userRole
        )
        {
            var validRoles = new[] { "admin", "advisor", "leader" };
            if (!validRoles.Contains(userRole.ToLower()))
                throw new UnauthorizedBusinessException(
                    "Bu işlem yalnızca admin, advisor veya leader kullanıcılar içindir."
                );

            var events = await _context
                .Event.Include(e => e.Club)
                .Include(e => e.EventParticipants)
                .Where(e => e.ClubId == clubId && e.EndDate < DateTime.Now)
                .OrderByDescending(e => e.EndDate)
                .ToListAsync();

            var result = events
                .Select(ev => new EventDto
                {
                    EventId = ev.EventId,
                    Name = ev.Name,
                    Description = ev.Description,
                    StartDate = ev.StartDate,
                    EndDate = ev.EndDate,
                    ClubId = ev.ClubId,
                    ClubName = ev.Club.Name,
                    MaxParticipants = ev.MaxParticipants,
                    ParticipantCount = ev.ParticipantCount,
                    ImageUrl = ev.ImageUrl,
                    Status = ev.Status,
                    Image = ev.Image != null ? Convert.ToBase64String(ev.Image) : null,
                    Participants = ev
                        .EventParticipants.Select(p => $"{p.User.Name} {p.User.Surname}")
                        .ToList(),
                    Category = ev.Category,
                    ResultReportDocumentId = ev.ResultReportDocumentId,
                    ResultReportStatus = !string.IsNullOrEmpty(ev.ResultReportDocumentId)
                        ? "yüklendi"
                        : (DateTime.Now > ev.EndDate.AddDays(15) ? "süre dolmuş" : "yüklenmedi"),
                })
                .ToList();

            return result;
        }

        public async Task<EventDto> SubmitEventResultReport(int eventId, EventResultReportDto dto, int userId, string userRole)
        {
            var validRoles = new[] { "leader", "admin" };
            if (!validRoles.Contains(userRole.ToLower()))
                throw new UnauthorizedBusinessException("Bu işlem yalnızca leader veya admin kullanıcılar içindir.");

            var eventEntity = await _context.Event.FirstOrDefaultAsync(e => e.EventId == eventId);
            if (eventEntity == null)
                throw new ResourceNotFoundException("Etkinlik bulunamadı.");

            var now = DateTime.Now;
            if (eventEntity.EndDate == null || now > eventEntity.EndDate.AddDays(15))
                throw new BusinessException("Etkinlik tamamlandıktan sonra 15 gün içinde sonuç bildiri formu yüklenmelidir.");

            if (dto.ReportFile != null && dto.ReportFile.Length > 0)
            {
                var ext = Path.GetExtension(dto.ReportFile.FileName);
                var fileName = $"result_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine("wwwroot", "uploads", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ReportFile.CopyToAsync(stream);
                }
                eventEntity.ResultReportDocumentId = fileName;
            }
            await _context.SaveChangesAsync();

            return new EventDto
            {
                EventId = eventEntity.EventId,
                Success = true,
                Message = "Etkinlik sonuç raporu başarıyla kaydedildi.",
                ResultReportDocumentId = eventEntity.ResultReportDocumentId,
                ResultReportStatus = !string.IsNullOrEmpty(eventEntity.ResultReportDocumentId)
                    ? "yüklendi"
                    : (now > eventEntity.EndDate.AddDays(15) ? "süre dolmuş" : "yüklenmedi"),
            };
        }

        private DateTime TrimToMinute(DateTime dt)
        {
            return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0, dt.Kind);
        }

        public async Task<bool> IsUserParticipatingInEvent(int userId, int eventId)
        {
            return await _context.EventParticipant.AnyAsync(ep =>
                ep.UserId == userId && ep.EventId == eventId
            );
        }

        private async Task<bool> CheckUserParticipation(int userId, int eventId)
        {
            return await _context.EventParticipant.AnyAsync(ep =>
                ep.UserId == userId && ep.EventId == eventId
            );
        }

        public async Task<List<EventParticipantDetailDto>> GetEventParticipants(int eventId)
        {
            var participants = await _context
                .EventParticipant.Where(ep => ep.EventId == eventId)
                .Join(
                    _context.Users,
                    ep => ep.UserId,
                    u => u.UserId,
                    (ep, u) =>
                        new EventParticipantDetailDto
                        {
                            UserId = u.UserId,
                            FullName = $"{u.Name} {u.Surname}",
                            RegisteredAt = ep.RegisteredAt,
                        }
                )
                .ToListAsync();

            return participants;
        }
        public async Task<List<object>> GetEventReservations(int eventId)
        {
            var reservations = await _context.EventReservations
                .Include(r => r.Location)
                .Include(r => r.TimeSlot)
                .Where(r => r.EventId == eventId)
                .Select(r => new
                {
                    r.Id,
                    LocationName = r.Location.Name,
                    Slot = r.TimeSlot.StartHour.ToString(@"hh\:mm")
                           + " - " +
                           r.TimeSlot.EndHour.ToString(@"hh\:mm")
                })
                .ToListAsync();

            return reservations.Cast<object>().ToList();
        }




    }
}
