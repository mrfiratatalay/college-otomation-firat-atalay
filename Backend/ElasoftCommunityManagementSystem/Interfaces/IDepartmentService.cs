using ElasoftCommunityManagementSystem.Dtos.DepartmentDtos;
using ElasoftCommunityManagementSystem.Models;

namespace ElasoftCommunityManagementSystem.Interfaces
{
    public interface IDepartmentService
    {
        Task<List<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> GetDepartmentByIdAsync(int id);
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto department);
        Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentUpdateDto department);
        Task<bool> DeleteDepartmentAsync(int id);
    }
} 