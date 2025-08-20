using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class DocumentModel
    {
        [Key]
        public string DocumentId { get; set; } = string.Empty;
        
        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;
        
        [Required, MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;
        
        [Required, MaxLength(512)]
        public string FilePath { get; set; } = string.Empty;
        
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        
        public int? UserId { get; set; }
        
        public string? EntityType { get; set; }
        
        [MaxLength(50)]
        public int? EntityId { get; set; }
        
        // Navigation property for user
        public UserModel? User { get; set; }
    }
}
