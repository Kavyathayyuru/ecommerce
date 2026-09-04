using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterUser(RegisterUserDto registerUserDto);

        Task<string?> Login(LoginDto loginDto);
    }
}