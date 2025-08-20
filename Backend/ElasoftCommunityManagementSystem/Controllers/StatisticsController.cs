using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Interfaces;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/statistics")]
    [ApiController]
    [Authorize(Roles = "admin")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            try
            {
                var result = await _statisticsService.GetSummaryStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "İstatistikler alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("clubs")]
        public async Task<IActionResult> GetClubStatistics()
        {
            try
            {
                var result = await _statisticsService.GetClubStatisticsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Topluluk istatistikleri alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("events")]
        public async Task<IActionResult> GetEventStatistics([FromQuery] int year)
        {
            try
            {
                var result = await _statisticsService.GetEventStatisticsAsync(year);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Etkinlik istatistikleri alınırken bir hata oluştu: " + ex.Message });
            }
        }

        [HttpGet("expenses")]
        public async Task<IActionResult> GetExpenseStatistics([FromQuery] string clubId = "all", [FromQuery] string dateRange = "all")
        {
            try
            {
                DateTime? startDate = null;
                DateTime? endDate = null;

                // Özel tarih aralığı parametreleri
                if (dateRange == "custom" && Request.Query.ContainsKey("startDate") && Request.Query.ContainsKey("endDate"))
                {
                    startDate = DateTime.Parse(Request.Query["startDate"]);
                    endDate = DateTime.Parse(Request.Query["endDate"]);
                }

                var result = await _statisticsService.GetExpenseStatisticsAsync(clubId, dateRange, startDate, endDate);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Harcama istatistikleri alınırken bir hata oluştu: " + ex.Message });
            }
        }
    }
} 