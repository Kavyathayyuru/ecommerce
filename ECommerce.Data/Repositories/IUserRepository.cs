using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetUserByEmail(string email);

        Task<List<User>> GetAllUsers();

        Task AddUser(User user);
    }
}