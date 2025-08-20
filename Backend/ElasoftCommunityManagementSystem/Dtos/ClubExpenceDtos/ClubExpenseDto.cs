namespace ElasoftCommunityManagementSystem.Dtos.ClubExpenceDtos
{
    public class ClubExpenseDto
    {
        public int Id { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }
        public decimal CashSupport { get; set; }
        public decimal InKindSupport { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public string? DokumanUrl { get; set; } // Belge URL'si
    }
}
