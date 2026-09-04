using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
     IUserRepository userRepository,
     IPasswordHasher<User> passwordHasher,
     IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        public async Task<bool> RegisterUser(RegisterUserDto registerUserDto)
        {
            var existingUser =
                await _userRepository.GetUserByEmail(registerUserDto.Email);

            if (existingUser != null)
            {
                return false;
            }

            var user = new User
            {
                Username = registerUserDto.Username,
                Email = registerUserDto.Email
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(user, registerUserDto.Password);

            await _userRepository.AddUser(user);

            return true;
        }

        public async Task<string?> Login(LoginDto loginDto)
        {
            var user =
                await _userRepository.GetUserByEmail(loginDto.Email);

            if (user == null)
            {
                return null;
            }

            var result =
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    loginDto.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var jwtKey = _configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured.");

            var jwtIssuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer is not configured.");

            var jwtAudience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException("JWT Audience is not configured.");

            var expiryMinutes = int.Parse(
                _configuration["Jwt:ExpiryMinutes"] ?? "60"
            );

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}