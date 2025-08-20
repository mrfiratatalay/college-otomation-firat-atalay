using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class EventReservation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int EventId { get; set; }
        public EventModel Event { get; set; }

        [Required]
        public int LocationId { get; set; }
        public Location Location { get; set; }

        [Required]
        public int TimeSlotId { get; set; }
        public DailySlot TimeSlot { get; set; }

        public DateTime ReservationDate { get; set; }
    }
}
