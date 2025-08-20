using ElasoftCommunityManagementSystem.DTOs;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IO;
using ElasoftCommunityManagementSystem.Interfaces;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/announcements")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AnnouncementController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: api/announcements
        [HttpGet]
        public async Task<IActionResult> GetAnnouncements([FromQuery] bool? isActive)
        {
            var query = _context.Announcement
                .Include(a => a.Club)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(a => a.IsActive == isActive.Value);
            }

            var announcements = await query.ToListAsync();
            return Ok(announcements);
        }

        // GET: api/announcements/club/{clubId}
        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetClubAnnouncements(int clubId, [FromQuery] bool? isActive)
        {
            var query = _context.Announcement
                .Include(a => a.Club)
                .Where(a => a.ClubId == clubId)
                .OrderByDescending(a => a.CreatedAt)
                .AsQueryable();

            if (isActive.HasValue)
            {
                query = query.Where(a => a.IsActive == isActive.Value);
            }

            var announcements = await query.ToListAsync();
            return Ok(announcements);
        }

        // GET: api/announcements/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAnnouncement(int id)
        {
            var announcement = await _context.Announcement
                .Include(a => a.Club)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);

            if (announcement == null)
                return NotFound("Duyuru bulunamadı.");

            return Ok(announcement);
        }

        // POST: api/announcements
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAnnouncement([FromBody] CreateAnnouncementDto dto)
        {
            var club = await _context.Club.FindAsync(dto.ClubId);
            if (club == null)
                return NotFound("Topluluk bulunamadı.");

            // Base64 formatındaki resmi kontrol et
            string imageUrl = null;
            if (!string.IsNullOrEmpty(dto.ImageUrl))
            {
                try
                {
                    // Base64 formatını doğrula
                    var base64Data = dto.ImageUrl.Split(',').Last();
                    var imageBytes = Convert.FromBase64String(base64Data);
                    
                    // Resmi kaydet (örneğin: wwwroot/images/announcements klasörüne)
                    var fileName = $"announcement_{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine("wwwroot", "images", "announcements", fileName);
                    
                    // Dizin yoksa oluştur
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    
                    // Resmi kaydet
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    
                    // Resmin URL'sini oluştur
                    imageUrl = $"/images/announcements/{fileName}";
                }
                catch (Exception ex)
                {
                    return BadRequest($"Resim yüklenirken hata oluştu: {ex.Message}");
                }
            }

            var announcement = new AnnouncementModel
            {
                ClubId = dto.ClubId,
                Title = dto.Title,
                Content = dto.Content,
                ImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Announcement.Add(announcement);
            await _context.SaveChangesAsync();

            // --- Bildirim Gönderme Başlangıcı ---
            try
            {
                // Kulübün onaylı üyelerinin ID'lerini al
                var memberIds = await _context.ClubMembership
                    .Where(m => m.ClubId == announcement.ClubId && m.Status.ToLower() == "approved") // Sadece onaylı üyeler
                    .Select(m => m.UserId)
                    .Distinct()
                    .ToListAsync();

                if (memberIds.Any())
                {
                    await _notificationService.CreateNotificationForMultipleUsersAsync(
                        memberIds,
                        $"Yeni Duyuru: {club.Name}",
                        $"{announcement.Title}",
                        "announcement",
                        announcement.AnnouncementId
                    );
                }
            }
            catch (Exception ex)
            {
                // Bildirim gönderirken hata olursa logla ama işlemi durdurma
                Console.WriteLine($"Bildirim gönderirken hata oluştu: {ex.Message}"); 
                // Daha gelişmiş loglama mekanizması kullanılabilir (ILogger vs.)
            }
            // --- Bildirim Gönderme Sonu ---

            return CreatedAtAction(nameof(GetAnnouncement), new { id = announcement.AnnouncementId }, announcement);
        }

        // PUT: api/announcements/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAnnouncement(int id, [FromBody] UpdateAnnouncementDto dto)
        {
            var announcement = await _context.Announcement.FindAsync(id);
            if (announcement == null)
                return NotFound("Duyuru bulunamadı.");

            if (dto.Title != null)
                announcement.Title = dto.Title;
            if (dto.Content != null)
                announcement.Content = dto.Content;
            
            // Resim güncelleme işlemi
            if (dto.ImageUrl != null)
            {
                try
                {
                    // Eski resmi sil
                    if (!string.IsNullOrEmpty(announcement.ImageUrl))
                    {
                        var oldFilePath = Path.Combine("wwwroot", announcement.ImageUrl.TrimStart('/'));
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // Yeni resmi kaydet
                    var base64Data = dto.ImageUrl.Split(',').Last();
                    var imageBytes = Convert.FromBase64String(base64Data);
                    
                    var fileName = $"announcement_{Guid.NewGuid()}.jpg";
                    var filePath = Path.Combine("wwwroot", "images", "announcements", fileName);
                    
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);
                    
                    announcement.ImageUrl = $"/images/announcements/{fileName}";
                }
                catch (Exception ex)
                {
                    return BadRequest($"Resim güncellenirken hata oluştu: {ex.Message}");
                }
            }

            if (dto.IsActive.HasValue)
                announcement.IsActive = dto.IsActive.Value;

            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(announcement);
        }

        // DELETE: api/announcements/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcement.FindAsync(id);
            if (announcement == null)
                return NotFound("Duyuru bulunamadı.");

            // Resmi sil
            if (!string.IsNullOrEmpty(announcement.ImageUrl))
            {
                var filePath = Path.Combine("wwwroot", announcement.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Announcement.Remove(announcement);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Duyuru başarıyla silindi." });
        }

        // PUT: api/announcements/{id}/toggle-status
        [HttpPut("{id}/toggle-status")]
        [Authorize]
        public async Task<IActionResult> ToggleAnnouncementStatus(int id)
        {
            var announcement = await _context.Announcement.FindAsync(id);
            if (announcement == null)
                return NotFound("Duyuru bulunamadı.");

            announcement.IsActive = !announcement.IsActive;
            announcement.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Duyuru durumu {(announcement.IsActive ? "aktif" : "pasif")} olarak güncellendi.", announcement });
        }

        [HttpGet("advisor")]
        [Authorize(Roles = "advisor")]
        public async Task<IActionResult> GetAdvisorAnnouncements()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
                return Unauthorized("Kullanıcı kimliği alınamadı.");

            var advisorClubIds = await _context.ClubMembership
                .Where(cm => cm.UserId == userId && cm.Role == "advisor" && cm.Status == "approved")
                .Select(cm => cm.ClubId)
                .ToListAsync();

            var announcements = await _context.Announcement
                .Where(a => advisorClubIds.Contains(a.ClubId))
                .Include(a => a.Club) // 👈 Burada Club'ı çekiyoruz
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new
                {
                    a.AnnouncementId,
                    a.ClubId,
                    ClubName = a.Club.Name, // 👈 Kulüp ismini alıyoruz
                    a.Title,
                    a.Content,
                    a.ImageUrl,
                    a.CreatedAt,
                    a.UpdatedAt,
                    a.IsActive
                })
                .ToListAsync();

            return Ok(announcements);
        }
    }
}