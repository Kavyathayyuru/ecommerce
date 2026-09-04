using ECommerce.Data.Repositories;

namespace ECommerce.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;

        public PaymentService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<bool> ProcessPayment(int orderId, int userId)
        {
            var order = await _orderRepository.GetOrderById(orderId);

            if (order == null)
                return false;

            if (order.UserId != userId)
                return false;

            if (order.Status != "Checkout")
                return false;

            // Simulate successful payment
            order.Status = "Paid";

            await _orderRepository.UpdateOrder(order);

            return true;
        }
    }
}