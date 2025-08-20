using ElasoftCommunityManagementSystem.Dtos.DepartmentDtos;
using ElasoftCommunityManagementSystem.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET: api/Departments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            try
            {
                var departments = await _departmentService.GetAllDepartmentsAsync();
                return Ok(departments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // GET: api/Departments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            try
            {
                var department = await _departmentService.GetDepartmentByIdAsync(id);
                return Ok(department);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // POST: api/Departments
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(DepartmentCreateDto departmentDto)
        {
            try
            {
                var createdDepartment = await _departmentService.CreateDepartmentAsync(departmentDto);
                return CreatedAtAction(nameof(GetDepartment), new { id = createdDepartment.Id }, createdDepartment);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // PUT: api/Departments/5
        [HttpPut("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateDepartment(int id, DepartmentUpdateDto departmentDto)
        {
            try
            {
                var updatedDepartment = await _departmentService.UpdateDepartmentAsync(id, departmentDto);
                return Ok(updatedDepartment);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // DELETE: api/Departments/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            try
            {
                var result = await _departmentService.DeleteDepartmentAsync(id);
                if (result)
                    return NoContent();
                return NotFound(new { message = $"Department with ID {id} not found" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
} 