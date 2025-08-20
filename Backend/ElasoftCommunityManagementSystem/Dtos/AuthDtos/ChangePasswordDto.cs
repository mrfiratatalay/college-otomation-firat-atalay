using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos.AuthDtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
} 