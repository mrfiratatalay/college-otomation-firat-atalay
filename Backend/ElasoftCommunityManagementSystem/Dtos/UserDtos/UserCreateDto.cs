using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos
{
    public class UserCreateDto
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Nationality { get; set; }

        [Required]
        public string Gender { get; set; }

        public string? DisabilityStatus { get; set; }

        public string StudentId { get; set; }

        public string Role { get; set; } = "user";
        
        public int? DepartmentId { get; set; }
    }
} 