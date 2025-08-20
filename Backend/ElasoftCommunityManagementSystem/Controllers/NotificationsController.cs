using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Dtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(INotificationService notificationService, ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcının okunmamış bildirimlerini getirir
        /// </summary>
        /// <returns>Okunmamış bildirimler ve sayısı</returns>
        [HttpGet("unread")]
        public async Task<IActionResult> GetUnreadNotifications()
        {
            try 
            {
                _logger.LogInformation("GetUnreadNotifications started");
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                _logger.LogInformation("UserIdClaim: {0}", userIdClaim?.Value);
                
                if (userIdClaim == null) 
                {
                    _logger.LogWarning("User ID claim not found");
                    return Unauthorized();
                }

                var userId = int.Parse(userIdClaim.Value);
                _logger.LogInformation("User ID parsed: {0}", userId);

                var result = await _notificationService.GetUnreadNotificationsAsync(userId);
                _logger.LogInformation("Result obtained with {0} notifications", result.Notifications.Count);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetUnreadNotifications");
                throw; // Middleware will handle this
            }
        }

        /// <summary>
        /// Tüm bildirimleri sayfalı şekilde getirir
        /// </summary>
        /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
        /// <param name="limit">Sayfa başına bildirim sayısı (varsayılan: 10)</param>
        /// <returns>Bildirimler listesi</returns>
        [HttpGet]
        public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int limit = 10)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var notifications = await _notificationService.GetNotificationsAsync(userId, page, limit);
                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in GetAllNotifications");
                throw;
            }
        }

        /// <summary>
        /// Bildirimi okundu olarak işaretler
        /// </summary>
        /// <param name="id">Bildirim ID</param>
        /// <returns>İşlem başarılı ise 204, bildirim bulunamazsa 404</returns>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var result = await _notificationService.MarkAsReadAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { message = "Bildirim bulunamadı" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MarkAsRead for notification {NotificationId}", id);
                throw;
            }
        }

        /// <summary>
        /// Tüm bildirimleri okundu olarak işaretler
        /// </summary>
        /// <returns>İşlem başarılı ise 204</returns>
        [HttpPut("mark-all-read")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var count = await _notificationService.MarkAllAsReadAsync(userId);
                return Ok(new { message = $"{count} bildirim okundu olarak işaretlendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in MarkAllAsRead");
                throw;
            }
        }

        /// <summary>
        /// Bildirimi siler
        /// </summary>
        /// <param name="id">Bildirim ID</param>
        /// <returns>İşlem başarılı ise 204, bildirim bulunamazsa 404</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNotification(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                if (userId == 0) return Unauthorized();

                var result = await _notificationService.DeleteNotificationAsync(id, userId);
                
                if (!result)
                {
                    return NotFound(new { message = "Bildirim bulunamadı" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred in DeleteNotification for notification {NotificationId}", id);
                throw;
            }
        }
    }
} 