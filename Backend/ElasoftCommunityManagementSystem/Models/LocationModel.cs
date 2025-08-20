using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class Location
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<TimeSlot> TimeSlots { get; set; }
        public ICollection<EventReservation> Reservations { get; set; }

        // ✅ Yeni Eklenen Alanlar
        [NotMapped] // EF Core desteklemediği için özel dönüşüm kullanacağız
        public List<int> ValidDays { get; set; } = new();

        [NotMapped]
        public List<DateTime> DisabledDates { get; set; } = new();

        // DB'de saklanacak alanlar (JSON string gibi)
        public string ValidDaysSerialized
        {
            get => string.Join(",", ValidDays);
            set => ValidDays = string.IsNullOrWhiteSpace(value)
                ? new List<int>()
                : value.Split(',').Select(int.Parse).ToList();
        }

        public string DisabledDatesSerialized
        {
            get => string.Join(";", DisabledDates.Select(d => d.ToString("o")));
            set => DisabledDates = string.IsNullOrWhiteSpace(value)
                ? new List<DateTime>()
                : value.Split(';').Select(DateTime.Parse).ToList();
        }
        public ICollection<DailySlot> DailySlots { get; set; }

    }
}
