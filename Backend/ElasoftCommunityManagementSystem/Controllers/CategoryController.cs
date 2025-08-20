using ElasoftCommunityManagementSystem.Dtos.CategoryDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/categories")]
    [ApiController]
    [Produces("application/json")]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoryController> _logger;

        public CategoryController(ICategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Tu00fcm kategorileri listeler
        /// </summary>
        [HttpGet("listele")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler listelenirken hata oluu015ftu");
                return StatusCode(500, "Kategoriler alu0131nu0131rken bir hata oluu015ftu.");
            }
        }

        /// <summary>
        /// Belirli bir kategoriyi ID'ye gu00f6re getirir
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);

                if (category == null)
                    return NotFound($"ID: {id} olan kategori bulunamadu0131.");

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id} olan kategori getirilirken hata oluu015ftu.", id);
                return StatusCode(500, "Kategori alu0131nu0131rken bir hata oluu015ftu.");
            }
        }

        /// <summary>
        /// Yeni bir kategori oluu015fturur
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto createCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdCategory = await _categoryService.CreateCategoryAsync(createCategoryDto);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategori oluu015fturulurken hata oluu015ftu");
                return StatusCode(500, "Kategori oluu015fturulurken bir hata oluu015ftu.");
            }
        }

        /// <summary>
        /// Mevcut bir kategoriyi gu00fcnceller
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto updateCategoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedCategory = await _categoryService.UpdateCategoryAsync(id, updateCategoryDto);

                if (updatedCategory == null)
                    return NotFound($"ID: {id} olan kategori bulunamadu0131.");

                return Ok(updatedCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id} olan kategori gu00fcncellenirken hata oluu015ftu", id);
                return StatusCode(500, "Kategori gu00fcncellenirken bir hata oluu015ftu.");
            }
        }

        /// <summary>
        /// Bir kategoriyi siler
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var result = await _categoryService.DeleteCategoryAsync(id);

                if (!result)
                    return NotFound($"ID: {id} olan kategori bulunamadu0131.");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ID {Id} olan kategori silinirken hata oluu015ftu", id);
                return StatusCode(500, "Kategori silinirken bir hata oluu015ftu.");
            }
        }
    }
}
