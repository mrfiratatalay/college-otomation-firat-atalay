using System;

namespace ElasoftCommunityManagementSystem.Dtos
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string StudentId { get; set; }
        public string Phone { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string? DisabilityStatus { get; set; }
        public string Department { get; set; }
        public int? DepartmentId { get; set; } 
        public string Role { get; set; }
        public string Status { get; set; }
        public DateTime JoinDate { get; set; }
        public string Avatar { get; set; }
    }
} 