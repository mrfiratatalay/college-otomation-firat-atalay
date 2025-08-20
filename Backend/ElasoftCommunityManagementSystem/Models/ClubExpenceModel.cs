using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class ClubExpenceModel
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Club")]
        public int ClubId { get; set; }

        public ClubModel Club { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CashSupport { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal InKindSupport { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        public string? DokumanUrl { get; set; }
    }
}
