using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class TimeSlot
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LocationId { get; set; }

        public Location? Location { get; set; }

        [Required]
        public TimeSpan StartHour { get; set; }

        [Required]
        public TimeSpan EndHour { get; set; }

        public ICollection<EventReservation>? Reservations { get; set; }
    }
}
