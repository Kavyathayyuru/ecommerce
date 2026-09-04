using ECommerce.Data.Context;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Data.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly ECommerceDbContext _context;

        public InventoryRepository(ECommerceDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetProductById(int productId)
        {
            return await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        public async Task UpdateStock(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}