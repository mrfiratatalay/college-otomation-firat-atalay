using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.DTOs
{
    public class CreateAnnouncementDto
    {
        [Required]
        public int ClubId { get; set; }

        [Required]
        [MinLength(5)]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        [MinLength(10)]
        public string Content { get; set; }

        public string? ImageUrl { get; set; }
    }

    public class UpdateAnnouncementDto
    {
        [MinLength(5)]
        [MaxLength(200)]
        public string? Title { get; set; }

        [MinLength(10)]
        public string? Content { get; set; }

        public string? ImageUrl { get; set; }

        public bool? IsActive { get; set; }
    }

} 