using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos
{
    public class ChangeRoleDto
    {
        [Required(ErrorMessage = "Rol alanı zorunludur")]
        public string Role { get; set; }
    }
} 