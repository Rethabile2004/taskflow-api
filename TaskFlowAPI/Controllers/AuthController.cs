using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        //Iconfiguration gives us access to appsettings.json values
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email.ToLower());
            if (existingUser != null)
            {
                // 409 conflict resource already exists
                return Conflict(new { message = "Account with this email already exists." });
            }
            // hash the password -  Bcrypt handles the salt automatically
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);
            // create the new user
            var user = new User
            {
                FullName = registerDto.FullName,
                CreatedAt = DateTime.UtcNow,
                Email = registerDto.Email.ToLower(),
                PasswordHash = passwordHash,
            };
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            var token = GenerateJwtToken(user);
            var expiryTime = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);
            return Ok(new AuthResponseDto
            {
                Email = registerDto.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryTime),
                FullName = registerDto.FullName,
                Token = token
            });
        }
        public string GenerateJwtToken(User user)
        {
            return "will-be-covered-in-the-next-session.";
        }
    }
}
