namespace ECommerce.Models.DTOs;

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public bool IsAdmin { get; set; }
}