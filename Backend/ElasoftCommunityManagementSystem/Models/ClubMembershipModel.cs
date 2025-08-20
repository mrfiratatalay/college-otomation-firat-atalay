using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class ClubMembershipModel
    {
        [Key]
        public int MembershipId { get; set; }

        [Required]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual UserModel User { get; set; }

        [Required]
        [ForeignKey("Club")]
        public int ClubId { get; set; }
        public virtual ClubModel Club { get; set; }

        [Required, MaxLength(50)]
        public string Role { get; set; } = "member";

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        [Required, MaxLength(50)]
        public string Status { get; set; } = "pending";
    }
}
