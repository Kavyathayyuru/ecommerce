using ECommerce.Models;

namespace ECommerce.Business.Services
{
    public interface IInventoryService
    {
        Task<bool> UpdateStock(int productId, int stock);
        Task<List<Product>> GetAllInventory();
        Task<List<Product>> GetLowStockProducts(int threshold);
        Task<bool> IncreaseStock(int productId, int quantity);
        Task<bool> DecreaseStock(int productId, int quantity);
    }
}
