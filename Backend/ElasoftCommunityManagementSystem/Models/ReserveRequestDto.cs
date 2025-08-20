public class ReserveRequestDto
{
    public int EventId { get; set; }
    public int LocationId { get; set; }
    public int TimeSlotId { get; set; }  // DailySlot.Id
    public string ReservationDate { get; set; } // yyyy-MM-dd
}
