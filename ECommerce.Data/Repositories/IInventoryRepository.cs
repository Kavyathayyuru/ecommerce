using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IInventoryRepository
    {
        Task<Product?> GetProductById(int productId);
        Task UpdateStock(Product product);
    }
}