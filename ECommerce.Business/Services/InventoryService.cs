using ECommerce.Data.Repositories;
using ECommerce.Models;

namespace ECommerce.Business.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<bool> UpdateStock(int productId, int stock)
        {
            if (stock < 0)
                return false;

            var product = await _inventoryRepository.GetProductById(productId);

            if (product == null)
                return false;

            product.Stock = stock;

            await _inventoryRepository.UpdateStock(product);

            return true;
        }
        public async Task<List<Product>> GetAllInventory()
        {
            return await _inventoryRepository.GetAllInventory();
        }
        public async Task<List<Product>> GetLowStockProducts(int threshold)
        {
            if (threshold < 0)
            {
                return new List<Product>();
            }

            return await _inventoryRepository.GetLowStockProducts(threshold);
        }
        public async Task<bool> IncreaseStock(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var product = await _inventoryRepository.GetProductById(productId);

            if (product == null)
            {
                return false;
            }

            await _inventoryRepository.IncreaseStock(product, quantity);

            return true;
        }
        public async Task<bool> DecreaseStock(int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return false;
            }

            var product = await _inventoryRepository.GetProductById(productId);

            if (product == null)
            {
                return false;
            }

            if (product.Stock < quantity)
            {
                return false;
            }

            await _inventoryRepository.DecreaseStock(product, quantity);

            return true;
        }
    }
}