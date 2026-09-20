using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IInventoryRepository
    {
        Task<Product?> GetProductById(int productId);
        Task UpdateStock(Product product);
        Task<List<Product>> GetAllInventory();
        Task<List<Product>> GetLowStockProducts(int threshold);
        Task IncreaseStock(Product product, int quantity);
        Task DecreaseStock(Product product, int quantity);
    }
}