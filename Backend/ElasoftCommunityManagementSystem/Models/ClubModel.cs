using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElasoftCommunityManagementSystem.Models
{
    public class ClubModel
    {
        [Key]
        public int ClubId { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } = "pending"; // "pending", "active", "inactive"

        public int MemberCount { get; set; } = 0; // Üye sayısını tutuyoruz
        public int EventCount { get; set; } = 0; // Etkinlik sayısını tutuyoruz
        public int? AdvisorId { get; set; }

        public int? CategoryId { get; set; }
        
        // Resim ve belge için byte array alanları
         public byte[]? Image { get; set; }
        public byte[]? Document { get; set; }


        // Navigation properties
        [ForeignKey("AdvisorId")]
        public virtual UserModel? Advisor { get; set; }
        public virtual CategoryModel? Category { get; set; }
        public virtual ICollection<ClubMembershipModel> ClubMemberships { get; set; }
        public virtual ICollection<EventModel> Events { get; set; }
    }
}
