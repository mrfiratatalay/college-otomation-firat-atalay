using ElasoftCommunityManagementSystem.Dtos.ClubExpenceDtos;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IClubExpenseService
    {
        Task<List<ClubExpenseDto>> GetAllExpensesAsync();
        Task<List<ClubExpenseDto>> GetExpensesByClubIdAsync(int clubId);
        Task<bool> AddExpenseAsync(CreateExpenseDto dto, int userId, string userRole);
        Task<bool> DeleteExpenseAsync(int id, int userId, string userRole);
        Task<ClubExpenseDto?> UpdateExpenseAsync(int id, CreateExpenseDto dto, int userId, string userRole); // Return nullable DTO
    }
}