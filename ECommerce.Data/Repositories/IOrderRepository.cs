using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IOrderRepository
    {
        Task<Order> CreateOrder(Order order);

        Task<List<Order>> GetOrdersByUserId(int userId);

        Task<List<Order>> GetAllOrders();

        Task<Order?> GetOrderById(int orderId);
        Task UpdateOrder(Order order);
    }
}