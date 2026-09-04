namespace ECommerce.Business.Services
{
    public interface IPaymentService
    {
        Task<bool> ProcessPayment(int orderId, int userId);
    }
}