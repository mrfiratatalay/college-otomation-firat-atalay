using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos.AuthDtos
{
    public class ResetPasswordDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Token { get; set; }

        [Required]
        [MinLength(6)]
        public required string NewPassword { get; set; }
    }
}