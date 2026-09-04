namespace ECommerce.Business.Services
{
    public interface IInventoryService
    {
        Task<bool> UpdateStock(int productId, int stock);
    }
}
