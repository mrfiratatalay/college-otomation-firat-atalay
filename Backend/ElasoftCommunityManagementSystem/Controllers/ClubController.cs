using ElasoftCommunityManagementSystem.Dtos.ClubDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/clubs")]
    [ApiController]
    public class ClubController : ControllerBase
    {
        private readonly IClubService _clubService;

        public ClubController(IClubService clubService)
        {
            _clubService = clubService;
        }

        [HttpGet("listele")]
        public async Task<IActionResult> GetClubs([FromQuery] string? status)
        {
            var clubs = await _clubService.GetClubsAsync(status);
            return Ok(clubs);
        }
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetClubById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();

                // ClubService'e yeni bir metot ekleyeceğiz
                var club = await _clubService.GetClubByIdAsync(id, userId, role);
                return Ok(new { message = "Kulüp detayları başarıyla getirildi.", club });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateClub(IFormFile image = null, IFormFile document = null)
        {
            try
            {
                // Tüm form verilerini log'la
                Console.WriteLine("=== FORM VERİLERİ ===");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"Form key: {key}, value: {Request.Form[key]}");
                }

                // Manuel model binding
                var clubDto = new CreateClubDto
                {
                    Name = Request.Form["name"],
                    Description = Request.Form["description"]
                };

                // CategoryId ve AdvisorId'yi parse et
                if (int.TryParse(Request.Form["categoryId"], out int categoryId))
                {
                    clubDto.CategoryId = categoryId;
                }

                if (int.TryParse(Request.Form["advisorId"], out int advisorId))
                {
                    clubDto.AdvisorId = advisorId;
                }

                // Kullanıcı ID'sini CreatorUserId'ye ekle
                int userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                clubDto.CreatorUserId = userId;

                // DTO verilerini log'la
                Console.WriteLine("=== DTO VERİLERİ ===");
                Console.WriteLine($"Name: {clubDto.Name}");
                Console.WriteLine($"Description: {clubDto.Description}");
                Console.WriteLine($"AdvisorId: {clubDto.AdvisorId}");
                Console.WriteLine($"CategoryId: {clubDto.CategoryId}");
                Console.WriteLine($"CreatorUserId: {clubDto.CreatorUserId}");

                var userRole = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                Console.WriteLine($"Kullanıcı rolü: {userRole}");

                // Tarih aralığı kontrolü
                var dbContext = (AppDbContext)HttpContext.RequestServices.GetService(typeof(AppDbContext));
                var start = dbContext.Settings.FirstOrDefault(x => x.Key == "ClubCreateStartDate")?.Value;
                var end = dbContext.Settings.FirstOrDefault(x => x.Key == "ClubCreateEndDate")?.Value;
                if (!string.IsNullOrEmpty(start) && !string.IsNullOrEmpty(end))
                {
                    var today = DateTime.UtcNow.Date;
                    var startDate = DateTime.Parse(start);
                    var endDate = DateTime.Parse(end);
                    if (today < startDate || today > endDate)
                    {
                        return BadRequest("Topluluk oluşturma sadece belirlenen tarihler arasında yapılabilir.");
                    }
                }

                // Dosya işleme
                if (image != null)
                {
                    Console.WriteLine($"Image dosyası alındı: {image.FileName}, boyut: {image.Length}");
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    clubDto.Image = memoryStream.ToArray();
                }

                if (document != null)
                {
                    Console.WriteLine($"Document dosyası alındı: {document.FileName}, boyut: {document.Length}");
                    using var memoryStream = new MemoryStream();
                    await document.CopyToAsync(memoryStream);
                    clubDto.Document = memoryStream.ToArray();
                }

                var result = await _clubService.CreateClubAsync(clubDto, userRole);
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA - CreateClub: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/club-member-count")]
        [Authorize]
        public async Task<IActionResult> UpdateMemberCount(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                var count = await _clubService.ClubMemberCountAsync(id, userId, role);
                return Ok(new { message = "Üye sayısı :", memberCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/club-event-count")]
        [Authorize]
        public async Task<IActionResult> UpdateEventCount(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();
                var count = await _clubService.ClubEventCountAsync(id, userId, role);
                return Ok(new { message = "Etkinlik sayısı:", eventCount = count });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/aktif-et")]
        [Authorize(Roles = "admin,advisor")]
        public async Task<IActionResult> ApproveClub(int id)
        {
            try
            {
                await _clubService.ApproveClubAsync(id);
                return Ok("Topluluk aktif hale getirildi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/reddet")]
        [Authorize(Roles = "admin,advisor")]
        public async Task<IActionResult> RejectClub(int id)
        {
            try
            {
                await _clubService.DeleteClubAsync(id);
                return Ok("Topluluk başvurusu reddedildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting club {id}: {ex.Message}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteClub(int id)
        {
            try
            {
                await _clubService.DeleteClubAsync(id);
                return Ok("Topluluk silindi.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}/guncelle")]
        [Authorize]
        public async Task<IActionResult> UpdateClub(int id, IFormFile image = null, IFormFile document = null)
        {
            try
            {
                // Tüm form verilerini log'la
                Console.WriteLine("=== FORM VERİLERİ ===");
                foreach (var key in Request.Form.Keys)
                {
                    Console.WriteLine($"Form key: {key}, value: {Request.Form[key]}");
                }

                // Manuel model binding
                var clubDto = new CreateClubDto
                {
                    Name = Request.Form["name"],
                    Description = Request.Form["description"]
                };

                // CategoryId ve AdvisorId'yi parse et
                if (int.TryParse(Request.Form["categoryId"], out int categoryId))
                {
                    clubDto.CategoryId = categoryId;
                }

                if (int.TryParse(Request.Form["advisorId"], out int advisorId))
                {
                    clubDto.AdvisorId = advisorId;
                }

                // DTO verilerini log'la
                Console.WriteLine("=== DTO VERİLERİ ===");
                Console.WriteLine($"Name: {clubDto.Name}");
                Console.WriteLine($"Description: {clubDto.Description}");
                Console.WriteLine($"AdvisorId: {clubDto.AdvisorId}");
                Console.WriteLine($"CategoryId: {clubDto.CategoryId}");

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var role = User.FindFirst(ClaimTypes.Role)?.Value?.ToLower();

                // Dosya işleme
                if (image != null)
                {
                    Console.WriteLine($"Image dosyası alındı: {image.FileName}, boyut: {image.Length}");
                    using var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    clubDto.Image = memoryStream.ToArray();
                }

                if (document != null)
                {
                    Console.WriteLine($"Document dosyası alındı: {document.FileName}, boyut: {document.Length}");
                    using var memoryStream = new MemoryStream();
                    await document.CopyToAsync(memoryStream);
                    clubDto.Document = memoryStream.ToArray();
                }

                var updated = await _clubService.UpdateClubAsync(id, clubDto, userId, role);
                return Ok(new { message = "Topluluk bilgileri başarıyla güncellendi.", data = updated });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HATA - UpdateClub: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner Exception Stack Trace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("advisor/my-clubs")]
        [Authorize(Roles = "advisor")]
        public async Task<IActionResult> GetMyAdvisorClubs()
        {
            try
            {
                int advisorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var clubs = await _clubService.GetClubsByAdvisorAsync(advisorId);
                return Ok(clubs);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("istatistikler")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetClubStatistics()
        {
            try
            {
                var stats = await _clubService.GetClubMemberStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return BadRequest($"İstatistik bilgileri alınamadı: {ex.Message}");
            }
        }

    }
}
