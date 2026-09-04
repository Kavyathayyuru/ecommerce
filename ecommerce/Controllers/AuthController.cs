using ECommerce.Business.Services;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
        {
            var result = await _authService.RegisterUser(registerUserDto);

            if (!result)
                return BadRequest("Email already exists.");

            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            var token = await _authService.Login(loginDto);

            if (token == null)
                return Unauthorized("Invalid email or password.");

            return Ok(new { token });
        }
    }
}