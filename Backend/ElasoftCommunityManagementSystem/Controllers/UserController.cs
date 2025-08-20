using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ElasoftCommunityManagementSystem.Dtos;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public UsersController(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        // [Authorize(Roles = "admin")] // Test için geçici olarak kaldırıldı
        public async Task<IActionResult> GetUsers(
            [FromQuery] string search = "",
            [FromQuery] string role = "all",
            [FromQuery] string status = "all",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10
        )
        {
            var query = _context.Users.AsQueryable();

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(search)
                    || u.Surname.ToLower().Contains(search)
                    || u.Email.ToLower().Contains(search)
                    || u.SchoolNumber != null && u.SchoolNumber.ToLower().Contains(search)
                );
            }

            // Apply role filter
            if (role != "all" && !string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u => u.Role == role);
            }

            // Apply status filter - we need to determine how status is stored
            // For now, assuming "active" means non-null role, but this may need adjustment
            if (status != "all")
            {
                if (status == "active")
                {
                    query = query.Where(u => u.Role != null);
                }
                else if (status == "inactive")
                {
                    // Define inactive criteria as needed
                    query = query.Where(u => u.Role == null);
                }
                // Add other status filters as needed
            }

            // Count total items for pagination
            var totalItems = await query.CountAsync();

            // Apply pagination
            var users = await query
                .Include(u => u.Department)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserResponseDto
                {
                    Id = u.UserId,
                    Name = $"{u.Name} {u.Surname}",
                    Email = u.Email,
                    StudentId = u.SchoolNumber,
                    Phone = u.PhoneNumber,
                    Department = u.Department != null ? u.Department.Name : "Belirsiz",
                    DepartmentId = u.DepartmentId,
                    Role = u.Role,
                    Status = u.Role != null ? "active" : "inactive",
                    JoinDate = u.CreatedAt,
                    // Avatar is not stored in UserModel, so using a default
                    Avatar = "/images/default-avatar.png",
                })
                .ToListAsync();

            return Ok(
                new
                {
                    Items = users,
                    TotalItems = totalItems,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                }
            );
        }

        // GET: api/Users/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetUser(int id)
        {
            // Check authorization - admin can view any user, users can only view themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserRole != "admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _context
                .Users.Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            // Get counts for community, events, and comments
            var communityCount = await _context
                .ClubMembership.Where(cm => cm.UserId == id)
                .CountAsync();

            var eventCount = 0; // Add relation to events if it exists
            var commentCount = 0; // Add relation to comments if it exists

            var userResponse = new UserDetailResponseDto
            {
                Id = user.UserId,
                Name = $"{user.Name} {user.Surname}",
                StudentId = user.SchoolNumber,
                Email = user.Email,
                Phone = user.PhoneNumber,
                Department = user.Department != null ? user.Department.Name : "Belirsiz",
                DepartmentId = user.DepartmentId,
                Role = user.Role,
                Status = user.Role != null ? "active" : "inactive",
                JoinDate = user.CreatedAt,
                Avatar = "/images/default-avatar.png", // Default avatar
                CommunityCount = communityCount,
                EventCount = eventCount,
                CommentCount = commentCount,
            };

            return Ok(userResponse);
        }

        // POST: api/Users
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email exists
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
            {
                return BadRequest(new { message = "Email is already in use" });
            }

            // Generate password hash
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Create new user
            var user = new UserModel
            {
                Name = userDto.FirstName.Trim(),
                Surname = userDto.LastName.Trim(),
                Email = userDto.Email.ToLower().Trim(),
                PhoneNumber = userDto.Phone.Trim(),
                SchoolNumber = userDto.StudentId?.Trim(),
                Password = passwordHash,
                Nationality = userDto.Nationality.Trim(),
                Gender = userDto.Gender.Trim(),
                DisabilityStatus = userDto.DisabilityStatus?.Trim(),
                Role = userDto.Role,
                DepartmentId = userDto.DepartmentId,
                CreatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.UserId },
                new
                {
                    Id = user.UserId,
                    Name = $"{user.Name} {user.Surname}",
                    Email = user.Email,
                }
            );
        }

        // PUT: api/Users/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Authorization check
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserRole != "admin" && currentUserId != id)
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Update fields
            user.Name = userDto.FirstName?.Trim() ?? user.Name;
            user.Surname = userDto.LastName?.Trim() ?? user.Surname;
            user.PhoneNumber = userDto.Phone?.Trim() ?? user.PhoneNumber;
            user.SchoolNumber = userDto.StudentId?.Trim() ?? user.SchoolNumber;

            // Departman ID güncelleme
            // --- Department ID Update and Validation ---
            if (userDto.DepartmentId.HasValue)
            {
                // Check if the provided DepartmentId actually exists in the Departments table
                var departmentExists = await _context.Departments.AnyAsync(d =>
                    d.DepartmentId == userDto.DepartmentId.Value
                );
                if (!departmentExists)
                {
                    // Return a specific error if the department ID is invalid
                    ModelState.AddModelError(
                        "DepartmentId",
                        $"Geçersiz Bölüm ID'si: {userDto.DepartmentId.Value}. Böyle bir bölüm bulunamadı."
                    );
                    return BadRequest(ModelState); // Send detailed error back
                }
                // If valid, update the user's DepartmentId
                user.DepartmentId = userDto.DepartmentId.Value;
            }
            else // If DepartmentId is explicitly set to null in the DTO
            {
                // Allow clearing the department if needed, set user's DepartmentId to null
                // Or, if Department should always be present, add validation in DTO or here.
                // Assuming clearing is allowed:
                user.DepartmentId = null;
            }

            // Only update password if provided
            if (!string.IsNullOrEmpty(userDto.Password))
            {
                // Check if CurrentPassword is provided
                if (string.IsNullOrEmpty(userDto.CurrentPassword))
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifre gereklidir.");
                    return BadRequest(ModelState);
                }

                // Verify CurrentPassword
                if (!BCrypt.Net.BCrypt.Verify(userDto.CurrentPassword, user.Password))
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifre yanlış.");
                    return BadRequest(ModelState);
                }

                // Optional: Add minimum password length check if needed
                // if (userDto.Password.Length < 6) {
                //     ModelState.AddModelError("Password", "Şifre en az 6 karakter olmalıdır.");
                //     return BadRequest(ModelState);
                // }
                user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            }

            // Only admin can change role
            if (currentUserRole == "admin" && userDto.Role != null && userDto.Role != user.Role)
            {
                // Optional: Validate if the provided role is valid ('admin', 'advisor', 'user', 'leader' etc.)
                var validRoles = new[] { "admin", "advisor", "leader", "user" }; // Adjust as needed
                if (!validRoles.Contains(userDto.Role.ToLower()))
                {
                    ModelState.AddModelError(
                        "Role",
                        $"Geçersiz rol: {userDto.Role}. İzin verilen roller: {string.Join(", ", validRoles)}"
                    );
                    return BadRequest(ModelState);
                }
                if (userDto.Role != user.Role) // Only update if role actually changed
                {
                    user.Role = userDto.Role;
                }
            }

            try
            {
                _context.Users.Update(user); // Explicitly mark entity as modified
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    return Conflict(
                        new
                        {
                            message = "Veri aynı anda başka bir kullanıcı tarafından güncellendi. Lütfen sayfayı yenileyip tekrar deneyin.",
                        }
                    );
                }
            }
            catch (Exception ex)
            {
                // Log the general exception
                // logger.LogError(ex, $"Error updating user {id}");
                return StatusCode(
                    500,
                    new
                    {
                        message = "Kullanıcı güncellenirken beklenmedik bir sunucu hatası oluştu.",
                    }
                );
            }

            return NoContent();
        }

        // DELETE: api/Users/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/Users/departments
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context
                .Departments.Select(d => new { Id = d.DepartmentId, Name = d.Name })
                .ToListAsync();

            return Ok(departments);
        }

        private async Task<bool> UserExists(int id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        // Öğrenci numarasına göre bölüm belirleme (örnek bir implementasyon)
        private string GetDepartmentFromSchoolNumber(string schoolNumber)
        {
            if (string.IsNullOrEmpty(schoolNumber))
                return "Belirsiz";

            // Öğrenci numarası formata göre departman eşleştirme
            // Bu kısmı kendi öğrenci numarası formatınıza göre düzenleyin
            if (schoolNumber.StartsWith("20"))
                return "Bilgisayar Mühendisliği";
            else if (schoolNumber.StartsWith("21"))
                return "Elektrik-Elektronik Mühendisliği";
            else if (schoolNumber.StartsWith("22"))
                return "Makine Mühendisliği";
            else if (schoolNumber.StartsWith("23"))
                return "Endüstri Mühendisliği";
            else
                return "Diğer Bölümler";
        }

        [HttpPut("{id}/role")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeRoleDto roleDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Kullanıcı bulunamadı" });
            }

            // Geçerli roller kontrolü
            var validRoles = new[] { "admin", "advisor", "leader", "user" };
            if (!validRoles.Contains(roleDto.Role))
            {
                return BadRequest(new { message = "Geçersiz rol" });
            }

            var result = await _userService.ChangeUserRoleAsync(id, roleDto.Role);
            if (!result)
            {
                return BadRequest(new { message = "Rol değiştirilemedi" });
            }

            return Ok(new { message = "Kullanıcı rolü başarıyla güncellendi" });
        }
        [HttpGet("department-distribution")]
        public async Task<IActionResult> GetClubDepartmentDistribution([FromQuery] int? clubId = null)
        {
            var result = await _userService.GetClubDepartmentDistributionAsync(clubId);
            return Ok(result);
        }


    }
}
