using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("admindashboard")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            var now = DateTime.UtcNow;

            var totalUsers = await _context.Users.CountAsync();
            var totalClubs = await _context.Club.CountAsync();
            var totalEvents = await _context.Event.CountAsync();

            var activeEvents = await _context.Event
                .Where(e => e.StartDate <= now && e.EndDate >= now)
                .CountAsync();

            var pendingApplications = await _context.ClubMembership
                .Where(m => m.Status == "Bekliyor")
                .CountAsync();

            var recentEvents = await _context.Event
                .OrderByDescending(e => e.CreatedAt)
                .Take(5)
                .Select(e => new
                {
                    e.Name,
                    e.StartDate,
                    e.EndDate
                })
                .ToListAsync();

            var recentApplications = await _context.ClubMembership
                .Where(m => m.Status == "Bekliyor")
                .OrderByDescending(m => m.JoinedAt)
                .Take(5)
                .Select(m => new
                {
                    UserName = m.User.Name,
                    ClubName = m.Club.Name,
                    m.JoinedAt
                })
                .ToListAsync();

            return Ok(new
            {
                totalUsers,
                totalClubs,
                totalEvents,
                activeEvents,
                pendingApplications,
                recentEvents,
                recentApplications
            });
        }

        [HttpGet("advisordashboard")]
        [Authorize(Roles = "advisor")]
        public async Task<IActionResult> GetAdvisorDashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var now = DateTime.UtcNow;

            var advisorClubs = await _context.ClubMembership
                .Include(m => m.Club)
                .Where(m =>
                    m.UserId == userId &&
                    m.Role.ToLower() == "advisor" &&
                    m.Status.ToLower() == "approved"
                )
                .Select(m => m.Club)
                .ToListAsync();

            if (!advisorClubs.Any())
                return Forbid("Danışman olduğunuz bir kulüp bulunamadı.");

            var summaries = new List<object>();

            foreach (var club in advisorClubs)
            {
                var clubId = club.ClubId;

                var totalMembers = await _context.ClubMembership
                    .Where(m => m.ClubId == clubId && m.Status.ToLower() == "approved")
                    .CountAsync();

                var totalEvents = await _context.Event
                    .Where(e => e.ClubId == clubId)
                    .CountAsync();

                var activeEvents = await _context.Event
                     .Where(e => e.ClubId == clubId &&
                e.Status.ToLower() == "approved" &&
                e.StartDate <= now &&
                e.EndDate >= now)
                     .CountAsync();

                var recentClubEvents = await _context.Event
                    .Where(e => e.ClubId == clubId)
                    .OrderByDescending(e => e.CreatedAt)
                    .Take(5)
                    .Select(e => new
                    {
                        e.Name,
                        e.StartDate,
                        e.EndDate
                    })
                    .ToListAsync();

                summaries.Add(new
                {
                    clubId,
                    clubName = club.Name,
                    totalMembers,
                    totalEvents,
                    activeEvents,
                    recentClubEvents
                });
            }

            return Ok(summaries);
        }

        [HttpGet("studentdashboard")]
        [Authorize]
        public async Task<IActionResult> GetStudentDashboard()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var now = DateTime.UtcNow;

                // Öğrencinin üye olduğu topluluklar
                var joinedClubs = await _context.ClubMembership
                    .Include(m => m.Club)
                    .Where(m =>
                        m.UserId == userId &&
                        m.Status != null &&
                        (m.Status.ToLower() == "onaylı" ||
                         m.Status.ToLower() == "approved" ||
                         m.Status.ToLower() == "active")
                    )
                    .Select(m => new {
                        clubId = m.Club.ClubId,
                        clubName = m.Club.Name
                    })
                    .ToListAsync();

                // Öğrencinin katıldığı toplam etkinlik sayısı
                var totalEvents = await _context.EventParticipant
                    .Where(ep => ep.UserId == userId)
                    .CountAsync();

                // Yaklaşan etkinlikler
                var upcomingEvents = await _context.Event
                    .Include(e => e.Club)
                    .Where(e =>
                        e.EndDate >= now &&
                        e.Club.ClubMemberships.Any(m =>
                            m.UserId == userId &&
                            m.Status != null &&
                            (m.Status.ToLower() == "onaylı" ||
                             m.Status.ToLower() == "approved" ||
                             m.Status.ToLower() == "active")
                        )
                    )
                    .OrderBy(e => e.StartDate)
                    .Take(5)
                    .Select(e => new {
                        eventId = e.EventId.ToString(),
                        name = e.Name,
                        clubName = e.Club.Name,
                        startDate = e.StartDate,
                        endDate = e.EndDate
                    })
                    .ToListAsync();

                var result = new
                {
                    upcomingEvents,
                    joinedClubs,
                    totalEvents
                };

                Console.WriteLine($"Dönen veri: {System.Text.Json.JsonSerializer.Serialize(result)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex}");
                throw;
            }
        }

        [HttpGet("studentdashboard/debug")]
        [Authorize]
        public async Task<IActionResult> GetStudentDashboardDebug()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var now = DateTime.UtcNow;

            // Tüm üyelikleri kontrol et
            var allMemberships = await _context.ClubMembership
                .Include(m => m.Club)
                .Where(m => m.UserId == userId)
                .Select(m => new {
                    m.ClubId,
                    ClubName = m.Club.Name,
                    m.Status,
                    m.Role
                })
                .ToListAsync();

            // Tüm etkinlik katılımlarını kontrol et
            var allEventParticipations = await _context.EventParticipant
                .Include(ep => ep.Event)
                .Where(ep => ep.UserId == userId)
                .Select(ep => new {
                    ep.EventId,
                    EventName = ep.Event.Name,
                    ep.Event.StartDate,
                    ep.Event.EndDate
                })
                .ToListAsync();

            // Yaklaşan tüm etkinlikleri kontrol et
            var allUpcomingEvents = await _context.Event
                .Include(e => e.Club)
                .Where(e => e.EndDate >= now)
                .Select(e => new {
                    e.EventId,
                    e.Name,
                    ClubName = e.Club.Name,
                    e.StartDate,
                    e.EndDate,
                    ClubMembers = e.Club.ClubMemberships
                        .Where(m => m.UserId == userId)
                        .Select(m => new { m.Status, m.Role })
                        .ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                UserId = userId,
                CurrentTime = now,
                Memberships = allMemberships,
                EventParticipations = allEventParticipations,
                UpcomingEvents = allUpcomingEvents
            });
        }
    }
}
