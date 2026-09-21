using ECommerce.Data.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public UserController(
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _configuration = configuration;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsersForAdmin()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        var adminEmails = _configuration
            .GetSection("Admin:Emails")
            .GetChildren()
            .Select(section => section.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value));

        if (email == null || !adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
            return Forbid();

        var users = await _userRepository.GetAllUsers();
        return Ok(users.Select(user => new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = adminEmails.Contains(user.Email, StringComparer.OrdinalIgnoreCase)
                ? "Admin"
                : "Customer"
        }));
    }
}