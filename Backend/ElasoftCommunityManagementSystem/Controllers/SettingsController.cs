using ElasoftCommunityManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/settings")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SettingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("club-create-dates")]
        public IActionResult GetClubCreateDates()
        {
            var start = _context.Settings.FirstOrDefault(x => x.Key == "ClubCreateStartDate")?.Value;
            var end = _context.Settings.FirstOrDefault(x => x.Key == "ClubCreateEndDate")?.Value;
            return Ok(new { start, end });
        }

        [HttpPost("club-create-dates")]
        [Authorize(Roles = "admin")]
        public IActionResult SetClubCreateDates([FromBody] ClubCreateDatesDto dto)
        {
            var startSetting = _context.Settings.FirstOrDefault(x => x.Key == "ClubCreateStartDate");
            var endSetting = _context.Settings.FirstOrDefault(x => x.Key == "ClubCreateEndDate");

            if (startSetting == null)
            {
                startSetting = new SettingModel { Key = "ClubCreateStartDate" };
                _context.Settings.Add(startSetting);
            }
            if (endSetting == null)
            {
                endSetting = new SettingModel { Key = "ClubCreateEndDate" };
                _context.Settings.Add(endSetting);
            }
            startSetting.Value = dto.StartDate;
            endSetting.Value = dto.EndDate;
            _context.SaveChanges();
            return Ok();
        }
    }

    public class ClubCreateDatesDto
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}