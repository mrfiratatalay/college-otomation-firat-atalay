using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos
{
    public class UserUpdateDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string StudentId { get; set; }
        public string? CurrentPassword { get; set; }
        public string? Password { get; set; }
        public string Role { get; set; }
        public int? DepartmentId { get; set; }
    }
} 