using ElasoftCommunityManagementSystem.Dtos.AdvisorDtos;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IAdvisorService
    {
        Task<List<AdvisorDto>> GetAllAsync();
        Task<List<AdvisorDto>> GetAvailableAsync();
        Task<List<AdvisorDto>> SearchAdvisorAsync(string name);
    }
}
