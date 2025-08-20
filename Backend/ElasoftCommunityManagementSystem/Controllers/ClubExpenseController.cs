using ElasoftCommunityManagementSystem.Dtos.ClubExpenceDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using ElasoftCommunityManagementSystem.Exceptions; // ResourceNotFoundException ve ValidationException için
using Microsoft.EntityFrameworkCore; // _context için
using System.Security.Claims; // Kullanıcı kimlik bilgileri için
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/club-expenses")]
    [ApiController]
    public class ClubExpenseController : ControllerBase
    {
        private readonly IClubExpenseService _expenseService;
        private readonly IWebHostEnvironment _env;

        public ClubExpenseController(IClubExpenseService expenseService, IWebHostEnvironment env)
        {
            _expenseService = expenseService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllExpenses()
        {
            var data = await _expenseService.GetAllExpensesAsync();
            return Ok(data);
        }

        [HttpGet("club/{clubId}")]
        public async Task<IActionResult> GetByClub(int clubId)
        {
            var data = await _expenseService.GetExpensesByClubIdAsync(clubId);
            return Ok(data);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddExpense([FromForm] CreateExpenseDto dto)
        {
            try
            {
                Console.WriteLine("🔍 Dokuman geldi mi?: " + (dto.Dokuman != null));//debug amaçlı eklendi
                Console.WriteLine("🔍 Dokuman dosya adı: " + (dto.Dokuman?.FileName ?? "YOK"));//
                string? dokumanUrl = null;

                if (dto.Dokuman != null && dto.Dokuman.Length > 0)
                {
                    var uploadDir = Path.Combine(_env.WebRootPath, "uploads");
                    if (!Directory.Exists(uploadDir))
                        Directory.CreateDirectory(uploadDir);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Dokuman.FileName)}";
                    var filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.Dokuman.CopyToAsync(stream);
                    }

                    dokumanUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
                    Console.WriteLine("✅ Dokuman URL oluşturuldu: " + dokumanUrl); // DEBUG AMAÇLI
                }

                dto.DokumanUrl = dokumanUrl; // Doküman URL DTO'ya 

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var userRole = User.FindFirst(ClaimTypes.Role).Value;

                var success = await _expenseService.AddExpenseAsync(dto, userId, userRole);

                if (!success)
                {
                    return BadRequest(new { message = "Gider eklenirken bir hata oluştu." });
                }

                return Ok(new { message = "Gider başarıyla eklendi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Sunucu hatası: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateExpense(int id, [FromBody] CreateExpenseDto dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var userRole = User.FindFirst(ClaimTypes.Role).Value;

                var updatedExpense = await _expenseService.UpdateExpenseAsync(id, dto, userId, userRole);

                if (updatedExpense == null)
                {
                    return NotFound(new { message = "Gider bulunamadı veya güncelleme yetkiniz yok." });
                }

                return Ok(updatedExpense);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Gider güncellenirken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var userRoleClaim = User.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || userRoleClaim == null)
            {
                return Unauthorized(new { message = "Kullanıcı kimlik bilgileri alınamadı." });
            }

            var userId = int.Parse(userIdClaim.Value);
            var userRole = userRoleClaim.Value;

            try
            {
                var success = await _expenseService.DeleteExpenseAsync(id, userId, userRole);
                if (!success)
                    return Forbid("Silme yetkiniz yok veya gider bulunamadı.");

                return Ok(new { message = "Gider başarıyla silindi." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (ElasoftCommunityManagementSystem.Exceptions.ResourceNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Gider silinirken bir hata oluştu: " + ex.Message });
            }
        }
    }
}
