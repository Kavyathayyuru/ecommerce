using ECommerce.Data.Repositories;

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
    }
}