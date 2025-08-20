namespace ElasoftCommunityManagementSystem.Dtos.AuthDtos
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class TokenDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
