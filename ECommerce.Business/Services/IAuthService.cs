using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(RegisterUserDto registerUserDto);

        Task<AuthResponseDto?> Login(LoginDto loginDto);
    }
}