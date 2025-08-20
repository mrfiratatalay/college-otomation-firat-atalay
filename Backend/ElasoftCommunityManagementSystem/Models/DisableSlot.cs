using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class DisabledSlot
    {
        [Key]
        public int Id { get; set; }

        public int LocationId { get; set; }
        public int SlotId { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("LocationId")]
        public Location Location { get; set; }

        [ForeignKey("SlotId")]
        public DailySlot Slot { get; set; }
    }
}
