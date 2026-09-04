using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmail(string email);

        Task AddUser(User user);
    }
}