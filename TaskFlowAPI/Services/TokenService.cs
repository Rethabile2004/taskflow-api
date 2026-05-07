using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerateToken(User user)
        {
            // step one define what goes inside the token
            var claims = new[]
            {
              // NameIdentifier - standart claim for the user's unique Id
              new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
              // Email - useful for display and lookup
              new Claim(ClaimTypes.Email,user.Email),
              // Name - display name
              new Claim(ClaimTypes.Name,user.FullName)
          };
            // step two create signin credentials
            var secretKey = _configuration["JwtSettings:SecretKey"]!;
            // convert to bytes and wrap it in SymmetricSecurityKey
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(expiryHours),
                signingCredentials: credentials
                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
    }
}
