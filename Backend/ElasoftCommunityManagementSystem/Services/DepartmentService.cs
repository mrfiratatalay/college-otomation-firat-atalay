using ElasoftCommunityManagementSystem.Dtos.DepartmentDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class DepartmentService : IDepartmentService
    {
        private readonly AppDbContext _context;

        public DepartmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DepartmentDto>> GetAllDepartmentsAsync()
        {
            return await _context.Departments
                .Select(d => new DepartmentDto
                {
                    Id = d.DepartmentId,
                    Name = d.Name,
                    CreatedAt = d.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                throw new KeyNotFoundException($"Department with ID {id} not found");

            return new DepartmentDto
            {
                Id = department.DepartmentId,
                Name = department.Name,
                CreatedAt = department.CreatedAt
            };
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto departmentDto)
        {
            // Check if department with same name exists
            if (await _context.Departments.AnyAsync(d => d.Name == departmentDto.Name))
                throw new InvalidOperationException($"Department with name '{departmentDto.Name}' already exists");

            var department = new DepartmentModel
            {
                Name = departmentDto.Name,
                CreatedAt = DateTime.UtcNow
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.DepartmentId,
                Name = department.Name,
                CreatedAt = department.CreatedAt
            };
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(int id, DepartmentUpdateDto departmentDto)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                throw new KeyNotFoundException($"Department with ID {id} not found");

            // Check if there's another department with the same name
            if (await _context.Departments.AnyAsync(d => d.Name == departmentDto.Name && d.DepartmentId != id))
                throw new InvalidOperationException($"Department with name '{departmentDto.Name}' already exists");

            department.Name = departmentDto.Name;

            await _context.SaveChangesAsync();

            return new DepartmentDto
            {
                Id = department.DepartmentId,
                Name = department.Name,
                CreatedAt = department.CreatedAt
            };
        }

        public async Task<bool> DeleteDepartmentAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return false;

            // Check if there are users in this department
            if (await _context.Users.AnyAsync(u => u.DepartmentId == id))
                throw new InvalidOperationException("Cannot delete department that has users assigned to it");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();
            return true;
        }
    }
} 