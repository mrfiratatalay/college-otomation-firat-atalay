namespace ElasoftCommunityManagementSystem.DTOs
{
    public class CreateDailySlotDto
    {
        public int LocationId { get; set; }
        public int DayOfWeek { get; set; } // 0 = Pazartesi
        public string StartHour { get; set; } // "08:00"
        public string EndHour { get; set; }   // "10:00"
    }
}
