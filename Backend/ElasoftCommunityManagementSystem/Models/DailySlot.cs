using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class DailySlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LocationId { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        [Required]
        public int DayOfWeek { get; set; } // 0: Pazartesi, 6: Pazar

        [Required]
        public TimeSpan StartHour { get; set; }

        [Required]
        public TimeSpan EndHour { get; set; }
    }
}
