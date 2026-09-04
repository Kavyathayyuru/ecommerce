using ECommerce.Data.Context;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ECommerceDbContext _context;

        public UserRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmail(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}