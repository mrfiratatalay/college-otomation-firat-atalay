using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class NotificationModel
    {
        [Key]
        public int NotificationId { get; set; }
        
        [Required]
        public int UserId { get; set; }
        
        [ForeignKey("UserId")]
        public UserModel User { get; set; }
        
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public string? Link { get; set; }
        
        public bool Read { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public string Type { get; set; }
        
        public int? EntityId { get; set; }
    }
} 