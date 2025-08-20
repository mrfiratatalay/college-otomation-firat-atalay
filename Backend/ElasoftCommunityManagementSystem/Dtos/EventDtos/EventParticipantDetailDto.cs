namespace ElasoftCommunityManagementSystem.Dtos.EventDtos
{
    public class EventParticipantDetailDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty; // Ad + Soyad
        public DateTime RegisteredAt { get; set; }
    }
}
