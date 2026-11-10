using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Data;
using UserService.Models;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly PasswordHasher<User> _passwordHasher = new();
        private readonly IConfiguration _configuration;

        public UsersController(UserDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id:int}")]
        [Authorize]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            if (!User.IsInRole("Admin") && User.FindFirstValue(JwtRegisteredClaimNames.Sub) != id.ToString())
            {
                return Forbid();
            }

            var user = await _context.Users.FindAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Register(RegisterUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                request.Password.Length < 8)
            {
                return BadRequest("Name, email, and a password of at least 8 characters are required.");
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(user => user.Email == email))
            {
                return Conflict("An account with this email already exists.");
            }

            var user = new User { Name = request.Name.Trim(), Email = email, Role = "Customer" };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPut("{id:int}")]
        [Authorize]
        public async Task<ActionResult<User>> UpdateUser(int id, UpdateUserRequest request)
        {
            if (!User.IsInRole("Admin") && User.FindFirstValue(JwtRegisteredClaimNames.Sub) != id.ToString())
            {
                return Forbid();
            }
            if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Name and email are required.");
            }

            var user = await _context.Users.FindAsync(id);
            if (user is null)
            {
                return NotFound();
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _context.Users.AnyAsync(existing => existing.Id != id && existing.Email == email))
            {
                return Conflict("An account with this email already exists.");
            }

            user.Name = request.Name.Trim();
            user.Email = email;
            await _context.SaveChangesAsync();
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized();
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _context.Users.SingleOrDefaultAsync(existing => existing.Email == email);
            if (user is null || _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return Unauthorized();
            }

            var expiresAtUtc = DateTime.UtcNow.AddHours(8);
            var token = CreateToken(user, expiresAtUtc);
            return Ok(new LoginResponse(
                new AuthenticatedUser(user.Id, user.Name, user.Email, user.Role),
                token,
                expiresAtUtc));
        }

        private string CreateToken(User user, DateTime expiresAtUtc)
        {
            var signingKey = _configuration["Authentication:SigningKey"]
                ?? throw new InvalidOperationException("Authentication signing key is not configured.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("role", user.Role)
            };
            var token = new JwtSecurityToken(
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public record RegisterUserRequest(string Name, string Email, string Password);
    public record UpdateUserRequest(string Name, string Email);
    public record LoginRequest(string Email, string Password);
    public record AuthenticatedUser(int Id, string Name, string Email, string Role);
    public record LoginResponse(AuthenticatedUser User, string Token, DateTime ExpiresAtUtc);
}