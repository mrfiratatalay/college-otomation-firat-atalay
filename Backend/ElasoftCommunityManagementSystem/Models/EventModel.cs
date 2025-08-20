using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class EventModel
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public int ClubId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        // ⏰ Başlangıç ve bitiş tarihleri
        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }

        [Required]
        public int MaxParticipants { get; set; } // 🧍 Katılımcı limiti

        public string? ImageUrl { get; set; } // 🖼️ Afiş görseli

        public byte[]? Image { get; set; } // Resim verisi (base64 için)

        public string? Category { get; set; }

        public string? ApplicationFormDocumentId { get; set; }
        public string? ManagementBoardFormDocumentId { get; set; }
        public string? ResultReportDocumentId { get; set; }

        public ClubModel Club { get; set; }
        public ICollection<EventParticipantModel> EventParticipants { get; set; }
        public int ParticipantCount { get; set; } = 0;
        public string Status { get; set; } = "advisor_pending"; // advisor_pending, admin_pending, approved, rejected
        public DateTime? UpdatedAt { get; set; }
        public string? RejectReason { get; set; } // Admin/red nedenini tutar
        public ICollection<EventReservation> Reservations { get; set; }
    }
}

