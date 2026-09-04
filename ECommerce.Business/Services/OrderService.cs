using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<Order?> CreateOrder(
            int userId,
            CreateOrderDto createOrderDto)
        {
            if (createOrderDto.Items == null ||
                createOrderDto.Items.Count == 0)
            {
                return null;
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                TotalAmount = 0
            };

            foreach (var item in createOrderDto.Items)
            {
                var product = await _productRepository
                    .GetProductById(item.ProductId);

                if (product == null)
                {
                    return null;
                }

                if (item.Quantity <= 0 ||
                    product.Stock < item.Quantity)
                {
                    return null;
                }

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                order.OrderItems.Add(orderItem);

                order.TotalAmount +=
                    product.Price * item.Quantity;

                product.Stock -= item.Quantity;

                await _productRepository
                    .UpdateProduct(product);
            }

            return await _orderRepository.CreateOrder(order);
        }

        public async Task<List<Order>> GetOrdersByUserId(int userId)
        {
            return await _orderRepository
                .GetOrdersByUserId(userId);
        }

        public async Task<Order?> GetOrderById(int orderId)
        {
            return await _orderRepository
                .GetOrderById(orderId);
        }
        public async Task<bool> CheckoutOrder(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderById(orderId);

            if (order == null)
                return false;

            if (order.UserId != userId)
                return false;

            if (order.Status != "Pending")
                return false;

            order.Status = "Checkout";

            await _orderRepository.UpdateOrder(order);

            return true;
        }
    }
}