public class DisableDailySlotsDto
{
    public int LocationId { get; set; }
    public string Date { get; set; } // yyyy-MM-dd
    public List<int> DisabledSlotIds { get; set; }
}
