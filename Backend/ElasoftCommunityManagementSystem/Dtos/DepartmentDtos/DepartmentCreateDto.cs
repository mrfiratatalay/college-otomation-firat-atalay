using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos.DepartmentDtos
{
    public class DepartmentCreateDto
    {
        [Required]
        public string Name { get; set; }
    }
} 