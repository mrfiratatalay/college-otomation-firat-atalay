using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Dtos.DepartmentDtos
{
    public class DepartmentUpdateDto
    {
        [Required]
        public string Name { get; set; }
    }
} 