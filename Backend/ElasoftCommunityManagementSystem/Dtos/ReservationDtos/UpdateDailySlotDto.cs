public class UpdateDailySlotDto
{
    public int LocationId { get; set; }
    public int DayOfWeek { get; set; } // 0: Pazar, 1: Pazartesi, ...
    public List<SlotDto> Slots { get; set; }

    public class SlotDto
    {
        public string StartHour { get; set; } // "08:00"
        public string EndHour { get; set; }   // "10:00"
    }
}
