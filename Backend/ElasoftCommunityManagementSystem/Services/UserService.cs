using ElasoftCommunityManagementSystem.Dtos.UserDtos;
using ElasoftCommunityManagementSystem.Exceptions;
using ElasoftCommunityManagementSystem.Interfaces;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ElasoftCommunityManagementSystem.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserModel?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<List<UserModel>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<bool> UpdateUserAsync(UserModel user)
        {
            var existingUser = await _context.Users.FindAsync(user.UserId);
            if (existingUser == null)
                return false;

            _context.Entry(existingUser).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ChangeUserRoleAsync(int userId, string role)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return false;

            user.Role = role;
            await _context.SaveChangesAsync();
            return true;
        }

        

        public async Task<bool> UpdateUserProfileAsync(int userId, string name, string surname, string phoneNumber)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                throw new BusinessException("User not found");

            user.Name = name;
            user.Surname = surname;
            user.PhoneNumber = phoneNumber;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<List<ClubDepartmentDistributionDto>> GetClubDepartmentDistributionAsync(int? clubId)
        {
            var query = from cm in _context.ClubMembership
                        join u in _context.Users on cm.UserId equals u.UserId
                        join d in _context.Departments on u.DepartmentId equals d.DepartmentId into deptJoin
                        from dept in deptJoin.DefaultIfEmpty()
                        where !clubId.HasValue || cm.ClubId == clubId
                        group cm by new
                        {
                            cm.ClubId,
                            DepartmentName = dept != null ? dept.Name : "Herhangi Bir Bölüme Kayıtlı Olmayan"
                        } into grp
                        orderby grp.Key.ClubId
                        select new ClubDepartmentDistributionDto
                        {
                            ClubId = grp.Key.ClubId,
                            DepartmentName = grp.Key.DepartmentName,
                            MemberCount = grp.Count()
                        };

            return await query.ToListAsync();
        }
    }
} 