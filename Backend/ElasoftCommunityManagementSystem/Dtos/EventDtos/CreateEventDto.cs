using Microsoft.AspNetCore.Http;

namespace ElasoftCommunityManagementSystem.Dtos.EventDtos
{
    public class CreateEventDto
    {
        public int ClubId { get; set; } // Hangi topluluk için etkinlik açılıyor?
        public string Name { get; set; } // Etkinlik adı
        public string Description { get; set; } // Açıklama

        public DateTime StartDate { get; set; } // Başlangıç
        public DateTime EndDate { get; set; }   // Bitiş
        public int MaxParticipants { get; set; } // Kontenjan
        public string? Status { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? ImageFile { get; set; }
        public string? Category { get; set; }
        public IFormFile? ApplicationFormFile { get; set; }
        public IFormFile? ManagementBoardFormFile { get; set; }
    }
}
