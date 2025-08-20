using System.ComponentModel.DataAnnotations;

namespace ElasoftCommunityManagementSystem.Models
{
    public class BudgetRequestModel
    {
        [Key]
        public int BudgetRequestId { get; set; }
 
        public int ClubId { get; set; }
        public decimal RequestedAmount { get; set; }
        public string Status { get; set; } // Enum: pending, approved, rejected
        public int? ApprovedBy { get; set; }
        public DateTime RequestedAt { get; set; }

        public ClubModel Club { get; set; }
        public UserModel Approver { get; set; }
    }
}
