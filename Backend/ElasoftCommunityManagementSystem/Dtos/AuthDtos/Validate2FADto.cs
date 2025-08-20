namespace ElasoftCommunityManagementSystem.Dtos.AuthDtos
{
    public class Validate2FADto
    {
        public int UserId { get; set; }
        public required string Code { get; set; }
    }
}