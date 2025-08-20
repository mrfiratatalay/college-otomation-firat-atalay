namespace ElasoftCommunityManagementSystem.Dtos.ClubMembershipoDtos
{
    public class ClubMemberListDto
    {
        public int UserId { get; set; }
        public int MembershipId { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public string Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public string ImageUrl { get; set; }
    }
}