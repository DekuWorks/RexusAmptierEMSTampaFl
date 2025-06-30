using Microsoft.AspNetCore.Mvc;
using RexusOps360.API.Models;
using RexusOps360.API.Services;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RexusOps360.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _configuration;

        public AuthController(IJwtService jwtService, IConfiguration configuration)
        {
            _jwtService = jwtService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            // Get user from mock data (in production, this would come from database)
            var user = GetMockUser(request.Username);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Verify password (in production, use proper password hashing)
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user);

            // Update last login
            user.LastLoginAt = DateTime.UtcNow;

            var response = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                Role = user.Role,
                TenantId = user.TenantId,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };

            return Ok(response);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            // Validate input
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Username and password are required" });
            }

            // Check if user already exists (in production, check database)
            if (GetMockUser(request.Username) != null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Create new user (in production, save to database)
            var passwordHash = HashPassword(request.Password);
            var newUser = new User
            {
                Id = 999, // Mock ID
                Username = request.Username,
                PasswordHash = passwordHash,
                Role = request.Role ?? "Dispatcher",
                TenantId = request.TenantId,
                FullName = request.FullName,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            // Generate token for new user
            var token = _jwtService.GenerateToken(newUser);

            var response = new LoginResponse
            {
                Token = token,
                Username = newUser.Username,
                Role = newUser.Role,
                TenantId = newUser.TenantId,
                ExpiresAt = DateTime.UtcNow.AddHours(8)
            };

            return Ok(response);
        }

        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            var user = GetCurrentUserFromToken();
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            return Ok(new
            {
                user.Id,
                user.Username,
                user.Role,
                user.TenantId,
                user.FullName,
                user.Email,
                user.LastLoginAt
            });
        }

        private User? GetCurrentUserFromToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            var token = authHeader.Substring("Bearer ".Length);
            var principal = _jwtService.ValidateToken(token);
            
            if (principal == null)
            {
                return null;
            }

            var username = principal.FindFirst(ClaimTypes.Name)?.Value;
            return GetMockUser(username ?? "");
        }

        private User? GetMockUser(string username)
        {
            // Mock users for demonstration
            var users = new List<User>
            {
                new User
                {
                    Id = 1,
                    Username = "admin",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin",
                    TenantId = "tampa-fl",
                    FullName = "System Administrator",
                    Email = "admin@rexusops360.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                },
                new User
                {
                    Id = 2,
                    Username = "dispatcher",
                    PasswordHash = HashPassword("dispatch123"),
                    Role = "Dispatcher",
                    TenantId = "tampa-fl",
                    FullName = "Emergency Dispatcher",
                    Email = "dispatch@rexusops360.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-15)
                },
                new User
                {
                    Id = 3,
                    Username = "responder",
                    PasswordHash = HashPassword("respond123"),
                    Role = "Responder",
                    TenantId = "tampa-fl",
                    FullName = "Emergency Responder",
                    Email = "responder@rexusops360.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                }
            };

            return users.FirstOrDefault(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private bool VerifyPassword(string password, string hash)
        {
            var hashedPassword = HashPassword(password);
            return hashedPassword == hash;
        }
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Role { get; set; }
        public string? TenantId { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
} 