namespace ElasoftCommunityManagementSystem.Models
{
    public class RefreshTokenModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedAt { get; set; }
        public virtual UserModel User { get; set; }
    }
}