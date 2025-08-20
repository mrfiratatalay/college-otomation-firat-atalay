using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElasoftCommunityManagementSystem.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ElasoftCommunityManagementSystem.Services
{
    public class JwtTokenGeneratorService
    {
        private readonly string _secret;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenGeneratorService(IConfiguration configuration)
        {
            _secret = configuration["Jwt:Secret"];
            _issuer = configuration["Jwt:Issuer"];
            _audience = configuration["Jwt:Audience"];
        }

        public string GenerateToken(UserModel user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role?.ToLower() ?? "user"), // Changed from "kullanıcı" to "user"
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
