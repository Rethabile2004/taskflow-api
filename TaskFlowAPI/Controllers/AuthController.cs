using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using TaskFlowAPI.Data;
using TaskFlowAPI.DTOs;
using TaskFlowAPI.Models;
using TaskFlowAPI.Services;

namespace TaskFlowAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            AppDbContext context,
            ITokenService tokenService,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
            _logger = logger;
        }

        // POST api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == registerDto.Email.ToLower());

            if (existingUser != null)
            {
                // Warning — not an error, just a duplicate attempt
                _logger.LogWarning(
                    "Registration attempt with already existing email: {Email}",
                    registerDto.Email);

                return Conflict(new { message = "An account with this email already exists." });
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var newUser = new User
            {
                FullName = registerDto.FullName,
                Email = registerDto.Email.ToLower(),
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(newUser);
            await _context.SaveChangesAsync();

            // Information — normal successful event worth recording
            _logger.LogInformation(
                "New user registered: {Email} (UserId: {UserId})",
                newUser.Email,
                newUser.Id);

            var token = _tokenService.GenerateToken(newUser);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
                FullName = newUser.FullName,
                Email = newUser.Email
            });
        }

        // POST api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email.ToLower());

            if (user == null)
            {
                // Warning — failed login attempt worth tracking
                _logger.LogWarning(
                    "Failed login attempt — email not found: {Email}",
                    loginDto.Email);

                return Unauthorized(new { message = "Invalid email or password." });
            }

            var passwordValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);

            if (!passwordValid)
            {
                _logger.LogWarning(
                    "Failed login attempt — wrong password for: {Email}",
                    loginDto.Email);

                return Unauthorized(new { message = "Invalid email or password." });
            }

            _logger.LogInformation(
                "User logged in: {Email} (UserId: {UserId})",
                user.Email,
                user.Id);

            var token = _tokenService.GenerateToken(user);
            var expiryHours = int.Parse(_configuration["JwtSettings:ExpiryHours"]!);

            return Ok(new AuthResponseDto
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(expiryHours),
                FullName = user.FullName,
                Email = user.Email
            });
        }
    }
}