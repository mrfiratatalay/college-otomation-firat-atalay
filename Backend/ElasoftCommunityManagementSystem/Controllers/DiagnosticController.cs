using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [ApiController]
    [Route("api/diagnostic")]
    public class DiagnosticController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(AppDbContext context, ILogger<DiagnosticController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok(new { Status = "Ok", Timestamp = DateTime.UtcNow });
        }

        [HttpGet("db-connection")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    return Ok(new { Status = "Database connection successful" });
                }
                else
                {
                    return StatusCode(500, new { Status = "Database connection failed" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return StatusCode(500, new 
                { 
                    Status = "Database connection error", 
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message
                });
            }
        }

        [HttpGet("test-event-creation")]
        public IActionResult TestEventCreation()
        {
            try
            {
                // Create a test event object to validate model binding
                var testEvent = new 
                {
                    ClubId = 1,
                    Name = "Test Event",
                    Description = "This is a test event for diagnostics",
                    StartDate = DateTime.UtcNow.AddDays(1),
                    EndDate = DateTime.UtcNow.AddDays(2),
                    MaxParticipants = 10,
                    EventType = "physical"
                };

                return Ok(new 
                { 
                    Status = "Test event object created successfully", 
                    TestObject = testEvent
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test event creation failed");
                return StatusCode(500, new { Status = "Test failed", Error = ex.Message });
            }
        }

        [HttpPost("update-roles")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateRoles()
        {
            try
            {
                // Advisor rolünü user rolüne çevirme
                var usersToUpdate = await _context.Users.Where(u => u.Role == "advisor").ToListAsync();
                
                foreach (var user in usersToUpdate)
                {
                    user.Role = "user";
                }
                
                await _context.SaveChangesAsync();
                
                return Ok(new { message = $"{usersToUpdate.Count} kullanıcının rolü advisor'dan user'a çevrildi." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
} 