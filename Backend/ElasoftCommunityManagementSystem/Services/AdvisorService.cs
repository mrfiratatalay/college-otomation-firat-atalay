using ElasoftCommunityManagementSystem.Dtos.AdvisorDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class AdvisorService : IAdvisorService
    {
        private readonly AppDbContext _context;

        public AdvisorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AdvisorDto>> GetAllAsync()
        {
            return await _context.Users
                .Where(u => u.Role == "advisor")
                .Select(u => new AdvisorDto
                {
                    AdvisorId = u.UserId,
                    FullName = u.Name + " " + u.Surname
                }).ToListAsync();
        }

        public async Task<List<AdvisorDto>> GetAvailableAsync()
        {
            var assignedAdvisorIds = await _context.ClubMembership
                .Where(m => m.Role == "advisor" && m.Status == "approved")
                .Select(m => m.UserId)
                .ToListAsync();

            return await _context.Users
                .Where(u => u.Role == "advisor" && !assignedAdvisorIds.Contains(u.UserId))
                .Select(u => new AdvisorDto
                {
                    AdvisorId = u.UserId,
                    FullName = u.Name + " " + u.Surname
                }).ToListAsync();
        }

        public async Task<List<AdvisorDto>> SearchAdvisorAsync(string name)
        {
            return await _context.Users
                .Where(u => u.Role == "advisor" &&
                            (u.Name + " " + u.Surname).ToLower().Contains(name.ToLower()))
                .Select(u => new AdvisorDto
                {
                    AdvisorId = u.UserId,
                    FullName = u.Name + " " + u.Surname
                }).ToListAsync();
        }
    }
}
