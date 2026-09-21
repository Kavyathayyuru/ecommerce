using ECommerce.Models;
using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public interface IOrderService
    {
        Task<Order?> CreateOrder(int userId, CreateOrderDto createOrderDto);

        Task<List<Order>> GetOrdersByUserId(int userId);

        Task<List<Order>> GetAllOrders();

        Task<Order?> GetOrderById(int orderId);
        Task<bool> CheckoutOrder(int orderId, int userId);
        Task<Order?> UpdateOrderStatus(int orderId, string status);
    }
}