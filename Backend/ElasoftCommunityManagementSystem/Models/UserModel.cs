using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class UserModel
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required]
        public required string Surname { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required, MinLength(6)]
        public required string Password { get; set; }

        [Required]
        public required string PhoneNumber { get; set; }

        [Required]
        public required string Nationality { get; set; }

        [Required]
        public required string Gender { get; set; }

        public string? DisabilityStatus { get; set; }

        public string? SchoolNumber { get; set; } // Artık zorunlu değil

        public string? Role { get; set; } // Artık zorunlu değil, admin ayarlayacak

        [Column(TypeName = "datetime2(0)")] 
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // 2FA related properties
        public string? TwoFactorSecret { get; set; }
        public bool TwoFactorEnabled { get; set; } = false;
        public DateTime? LastTwoFactorSetup { get; set; }

        // Password reset related properties
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
        
        // Department ilişkisi
        public int? DepartmentId { get; set; }
        
        [ForeignKey("DepartmentId")]
        public virtual DepartmentModel? Department { get; set; }
    }
}
