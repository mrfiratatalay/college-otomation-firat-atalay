using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using ElasoftCommunityManagementSystem.Dtos.CategoryDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ElasoftCommunityManagementSystem.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();

                return categories.Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler getirilirken hata oluu015ftu");
                throw;
            }
        }

        public async Task<CategoryDto> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                    return null;

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id} olan kategori getirilirken hata oluu015ftu", id);
                throw;
            }
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto createCategoryDto)
        {
            try
            {
                var category = new CategoryModel
                {
                    Name = createCategoryDto.Name,
                    CreatedAt = DateTime.UtcNow,
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori oluu015fturulurken hata oluu015ftu");
                throw;
            }
        }

        public async Task<CategoryDto> UpdateCategoryAsync(
            int id,
            UpdateCategoryDto updateCategoryDto
        )
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                    return null;

                category.Name = updateCategoryDto.Name;
                category.UpdatedAt = DateTime.UtcNow;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "ID {Id} olan kategori gu00fcncellenirken hata oluu015ftu",
                    id
                );
                throw;
            }
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories.FindAsync(id);

                if (category == null)
                    return false;

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id} olan kategori silinirken hata oluu015ftu", id);
                throw;
            }
        }
    }
}
