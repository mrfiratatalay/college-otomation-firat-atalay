namespace ElasoftCommunityManagementSystem.Dtos.ClubExpenceDtos
{
    public class CreateExpenseDto
    {
        public int ClubId { get; set; }
                public decimal CashSupport { get; set; }
        public decimal InKindSupport { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Doküman URL'si (opsiyonel olabilir)
        public string? DokumanUrl { get; set; }
        public IFormFile? Dokuman { get; set; }
    }
}
