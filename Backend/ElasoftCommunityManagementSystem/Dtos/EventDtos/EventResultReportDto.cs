using Microsoft.AspNetCore.Http;

namespace ElasoftCommunityManagementSystem.Dtos.EventDtos
{
    public class EventResultReportDto
    {
        public int EventId { get; set; }
        public IFormFile ReportFile { get; set; }
    }
} 