namespace ElasoftCommunityManagementSystem.Dtos.EventDtos
{
    public class EventDto
    {
        public int? EventId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ClubName { get; set; }
        public int? ClubId { get; set; }
        public int? MaxParticipants { get; set; }
        public int? ParticipantCount { get; set; }  // Katılımcı sayısı eklendi
        public string? ImageUrl { get; set; }
        public string? Image { get; set; }
        public string? Status { get; set; } // Yaklaşan, Devam Eden, Tamamlandı, Dolu
        public bool Success { get; set; }
        public string? Message { get; set; }
        public bool IsParticipating { get; set; }
        public List<string>? Participants { get; set; } // Admin ve Danışman için
        public string? Category { get; set; }
        public string? RejectReason { get; set; }
        public string? ApplicationFormDocumentId { get; set; }
        public string? ManagementBoardFormDocumentId { get; set; }
        public string? ResultReportDocumentId { get; set; }
        public string ResultReportStatus { get; set; }
    }
}
