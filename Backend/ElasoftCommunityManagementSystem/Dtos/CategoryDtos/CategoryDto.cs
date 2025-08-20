using System;

namespace ElasoftCommunityManagementSystem.Dtos.CategoryDtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateCategoryDto
    {
        public string Name { get; set; }
    }

    public class UpdateCategoryDto
    {
        public string Name { get; set; }
    }
}
