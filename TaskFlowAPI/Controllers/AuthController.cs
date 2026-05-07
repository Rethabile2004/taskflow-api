using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Services;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController:ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        //Iconfiguration gives us access to appsettings.json values
        private readonly IConfiguration _configuration;
        public AuthController(AppDbContext context, IConfiguration configuration,ITokenService tokenService)
        {
            _context = context;
            _configuration = configuration;
            _tokenService=tokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            // check if email already exists
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email.Equals(registerDto.Email, StringComparison.CurrentCultureIgnoreCase));
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
            var token = _tokenService.GenerateToken(user);
            var expiryTime = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);
            return Ok(new AuthResponseDto
            {
                Email = registerDto.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryTime),
                FullName = registerDto.FullName,
                Token = token
            });
        }
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>>Login(LoginDto loginDto)
        {
            // find user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            // verify the password against the stored hash
            var passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
            if (!passwordValid)
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }
            var token = _tokenService.GenerateToken(user);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);
            return Ok(new AuthResponseDto
            {
                Email= loginDto.Email,
                ExpiresAt=DateTime.UtcNow.AddHours(expiryHours),
                FullName=user.FullName,
                Token=token
            });
        }
    }
}
