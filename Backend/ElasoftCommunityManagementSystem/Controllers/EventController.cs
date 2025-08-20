using ElasoftCommunityManagementSystem.Dtos;
using ElasoftCommunityManagementSystem.Dtos.EventDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/events")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // ✅ 1. Etkinlikleri listele
        [HttpGet("listele")]
        public async Task<IActionResult> GetEvents([FromQuery] int? clubId, [FromQuery] string? search)
        {
            // Get user data from token
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

            // Call IEventService with those parameters
            var result = await _eventService.GetEvents(clubId, userId, userRole, search);

            return Ok(result);
        }

        // ✅ 2. Etkinlik oluştur
        [HttpPost("ekle")]
        [Authorize]
        public async Task<IActionResult> CreateEvent([FromForm] CreateEventDto eventDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { Success = false, Message = "Validation failed", Errors = errors });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();

                // Ensure EventType is set if missing


                var result = await _eventService.CreateEvent(eventDto, userId, userRole);

                if (!result.Success)
                    return BadRequest(result);

                return Ok(result);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                // Log the specific exception
                Console.WriteLine($"Error creating event: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return StatusCode(500, new { Success = false, Message = "An error occurred while creating the event.", Details = ex.Message });
            }
        }

        // ✅ 3. Güncelle
        [HttpPut("{id}/update")]
        [Authorize]
        public async Task<IActionResult> UpdateEvent(int id, [FromForm] CreateEventDto eventDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            var result = await _eventService.UpdateEvent(id, eventDto, userId, userRole);

            if (!result.Success)
                return Forbid(result.Message);

            return Ok(result);
        }

        // ✅ 4. Sil
        [HttpDelete("{id}/delete")]
        [Authorize]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            var result = await _eventService.DeleteEvent(id, userId, userRole);

            if (!result.Success)
                return Forbid(result.Message);

            return Ok(result);
        }

        // ✅ 5. Katıl
        [HttpPost("{eventId}/katil")]
        [Authorize]
        public async Task<IActionResult> JoinEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _eventService.JoinEvent(eventId, userId);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        // ✅ 6. Ayrıl
        [HttpDelete("{eventId}/ayril")]
        [Authorize]
        public async Task<IActionResult> LeaveEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var result = await _eventService.LeaveEvent(eventId, userId);

            if (!result.Success)
                return NotFound(result);

            return Ok(result);
        }
        [HttpGet("yetkili-etkinlikler")]
        [Authorize(Roles = "advisor,leader")]
        public async Task<IActionResult> GetEventsForAuthorizedUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            var result = await _eventService.GetEventsForAuthorizedUser(userId, role);
            return Ok(result);
        }
        [HttpPut("{eventId}/advisor-approve")]
        [Authorize(Roles = "advisor")]
        public async Task<IActionResult> AdvisorApproveEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            await _eventService.ApproveOrRejectEvent(eventId, userId, "approved", userRole);
            return Ok(new { message = "Etkinlik danışman tarafından onaylandı. Şimdi admin onayında." });
        }

        [HttpPut("{eventId}/advisor-reject")]
        [Authorize(Roles = "advisor")]
        public async Task<IActionResult> AdvisorRejectEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            await _eventService.ApproveOrRejectEvent(eventId, userId, "rejected", userRole);
            return Ok(new { message = "Etkinlik danışman tarafından reddedildi." });
        }

        [HttpPut("{eventId}/admin-approve")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminApproveEvent(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            await _eventService.ApproveOrRejectEvent(eventId, userId, "approved", userRole);
            return Ok(new { message = "Etkinlik admin tarafından onaylandı." });
        }

        [HttpPut("{eventId}/admin-reject")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> AdminRejectEvent(int eventId, [FromBody] RejectReasonDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
            await _eventService.ApproveOrRejectEvent(eventId, userId, "rejected", userRole, dto?.RejectReason);
            return Ok(new { message = "Etkinlik admin tarafından reddedildi." });
        }

        // Kullanıcının etkinliğe katılım durumunu kontrol et
        [HttpGet("{eventId}/check-participation")]
        [Authorize]
        public async Task<IActionResult> CheckParticipation(int eventId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var isParticipating = await _eventService.IsUserParticipatingInEvent(userId, eventId);
            return Ok(new { isParticipating = isParticipating });
        }
        // ✅ Etkinliğe katılan kullanıcıları getir
        [HttpGet("{eventId}/participants")]
        public async Task<IActionResult> GetEventParticipants(int eventId)
        {
            var participants = await _eventService.GetEventParticipants(eventId);
            return Ok(participants);
        }

        // Get completed events for a specific club
        [HttpGet("completed/{clubId}")]
        [Authorize(Roles = "admin,advisor,leader")]
        public async Task<IActionResult> GetCompletedEvents(int clubId)
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
                
                var events = await _eventService.GetCompletedEvents(clubId, userId, userRole);
                return Ok(events);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Tamamlanan etkinlikler getirilirken bir hata oluştu.", Details = ex.Message });
            }
        }
        
        // Submit event result report
        [HttpPost("{eventId}/result-report")]
        [Authorize(Roles = "leader,admin")]
        public async Task<IActionResult> SubmitEventResultReport(int eventId, [FromForm] EventResultReportDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { Success = false, Message = "Validation failed", Errors = errors });
                }
                
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var userRole = User.FindFirstValue(ClaimTypes.Role)?.ToLower();
                
                dto.EventId = eventId;
                var result = await _eventService.SubmitEventResultReport(eventId, dto, userId, userRole);
                
                return Ok(result);
            }
            catch (BusinessException ex)
            {
                return StatusCode((int)ex.StatusCode, new { Success = false, Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Sonuç raporu yüklenirken bir hata oluştu.", Details = ex.Message });
            }
        }
        
        // Download document endpoint
        [HttpGet("documents/{documentId}")]
        [Authorize]
        public async Task<IActionResult> GetEventDocument(string documentId, [FromServices] IDocumentService documentService)
        {
            try
            {
                var (fileStream, contentType, fileName) = await documentService.GetDocumentAsync(documentId);
                return File(fileStream, contentType, fileName);
            }
            catch (FileNotFoundException)
            {
                return NotFound(new { Success = false, Message = "Belge bulunamadı." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "Belge indirilirken bir hata oluştu.", Details = ex.Message });
            }
        }
        //  Etkinliğe ait rezervasyonları getir
        [HttpGet("{eventId}/reservations")]
        [Authorize] // rol gerekmiyorsa bu satırı kaldırabilirsin
        public async Task<IActionResult> GetEventReservations(int eventId)
        {
            var reservations = await _eventService.GetEventReservations(eventId);
            return Ok(reservations);
        }


    }
}
