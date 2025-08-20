using ElasoftCommunityManagementSystem.Dtos.AdvisorDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvisorsController : ControllerBase
    {
        private readonly IAdvisorService _advisorService;

        public AdvisorsController(IAdvisorService advisorService)
        {
            _advisorService = advisorService;
        }

        // Tüm danışmanları getir
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var advisors = await _advisorService.GetAllAsync();
            return Ok(new
            {
                message = "Tüm danışmanlar listelendi.",
                total = advisors.Count,
                advisors
            });
        }

        // Henüz kulübe atanmadıysa getir
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable()
        {
            var advisors = await _advisorService.GetAvailableAsync();

            return Ok(new
            {
                message = advisors.Any()
                    ? "Kulübe atanmamış danışmanlar:"
                    : "Kulübe atanmamış danışman bulunamadı.",
                total = advisors.Count,
                advisors
            });
        }

        // İsimle danışman ara
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Arama kriteri boş olamaz.");

            var advisors = await _advisorService.SearchAdvisorAsync(name);

            if (!advisors.Any())
                return NotFound($"'{name}' ismine uygun danışman bulunamadı.");

            return Ok(new
            {
                message = $"'{name}' ismine uygun danışmanlar:",
                advisors
            });
        }
    }
}
