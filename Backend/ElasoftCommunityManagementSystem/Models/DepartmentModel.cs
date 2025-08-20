using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class DepartmentModel
    {
        [Key]
        public int DepartmentId { get; set; }

        [Required]
        public required string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // İlişkisel özellik - Bu departmana bağlı kullanıcılar
        public virtual ICollection<UserModel>? Users { get; set; }
    }
} 