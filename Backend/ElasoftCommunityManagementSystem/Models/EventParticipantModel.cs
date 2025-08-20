using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class EventParticipantModel
    {
       [Key]
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int UserId { get; set; }
        public DateTime RegisteredAt { get; set; }

        public EventModel Event { get; set; }
        public UserModel User { get; set; }
    }
}
