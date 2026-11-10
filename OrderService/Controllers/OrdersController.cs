using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using OrderService.Data;
using OrderService.Models;

namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public OrdersController(
            OrderDbContext context,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // GET: api/Orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            return await _context.Orders.ToListAsync();
        }

        // GET: api/Orders/1
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
        {
            if (request.UserId <= 0 || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest("A valid user, product, and positive quantity are required.");
            }

            var productResult = await GetProduct(request.ProductId);
            if (productResult.Error is not null)
            {
                return productResult.Error;
            }
            if (productResult.Product is null)
            {
                return NotFound("Product not found.");
            }
            if (request.Quantity > productResult.Product.Stock)
            {
                return Conflict("Requested quantity exceeds available stock.");
            }

            var order = new Order
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                TotalPrice = productResult.Product.Price * request.Quantity,
                Status = "Pending"
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id },
                order
            );
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutRequest request)
        {
            if (request.UserId <= 0 || request.ProductId <= 0 || request.Quantity <= 0 ||
                string.IsNullOrWhiteSpace(request.PaymentMethod))
            {
                return BadRequest("A valid user, product, positive quantity, and payment method are required.");
            }

            var userResult = await GetUser(request.UserId);
            if (userResult.Error is not null)
            {
                return userResult.Error;
            }
            if (userResult.User is null)
            {
                return NotFound("User not found.");
            }

            var productResult = await GetProduct(request.ProductId);
            if (productResult.Error is not null)
            {
                return productResult.Error;
            }
            if (productResult.Product is null)
            {
                return NotFound("Product not found.");
            }
            if (request.Quantity > productResult.Product.Stock)
            {
                return Conflict("Requested quantity exceeds available stock.");
            }

            var order = new Order
            {
                UserId = request.UserId,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                TotalPrice = productResult.Product.Price * request.Quantity,
                Status = "Pending"
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var paymentResult = await CreatePayment(order.Id, order.TotalPrice, request.PaymentMethod);
            if (paymentResult.Error is not null)
            {
                return paymentResult.Error;
            }

            order.Status = "Completed";
            await _context.SaveChangesAsync();

            return Ok(new CheckoutResponse(order, paymentResult.Payment!));
        }

        // PUT: api/Orders/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderRequest request)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status is "Cancelled" or "Completed")
            {
                return Conflict("Completed or cancelled orders cannot be updated.");
            }
            if (request.ProductId <= 0 || request.Quantity <= 0)
            {
                return BadRequest("A valid product and positive quantity are required.");
            }

            var productResult = await GetProduct(request.ProductId);
            if (productResult.Error is not null)
            {
                return productResult.Error;
            }
            if (productResult.Product is null)
            {
                return NotFound("Product not found.");
            }
            if (request.Quantity > productResult.Product.Stock)
            {
                return Conflict("Requested quantity exceeds available stock.");
            }

            order.ProductId = request.ProductId;
            order.Quantity = request.Quantity;
            order.TotalPrice = productResult.Product.Price * request.Quantity;

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            var allowedStatuses = new[] { "Pending", "Processing", "Completed", "Cancelled" };
            if (!allowedStatuses.Contains(request.Status, StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest("Status must be Pending, Processing, Completed, or Cancelled.");
            }
            if (order.Status == "Cancelled" || order.Status == "Completed")
            {
                return Conflict("Completed or cancelled orders cannot change status.");
            }

            order.Status = request.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)
                ? "Cancelled"
                : request.Status;
            await _context.SaveChangesAsync();
            return Ok(order);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Completed")
            {
                return Conflict("Completed orders cannot be cancelled.");
            }

            order.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(order);
        }

        private async Task<(CatalogProduct? Product, IActionResult? Error)> GetProduct(int productId)
        {
            var productServiceUrl = _configuration["Services:ProductServiceUrl"];
            if (string.IsNullOrWhiteSpace(productServiceUrl))
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "Product service URL is not configured."));
            }

            try
            {
                var response = await _httpClientFactory.CreateClient().GetAsync(
                    $"{productServiceUrl.TrimEnd('/')}/api/Products/{productId}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (null, null);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return (null, StatusCode((int)HttpStatusCode.BadGateway, "Product service could not validate the product."));
                }

                return (await response.Content.ReadFromJsonAsync<CatalogProduct>(), null);
            }
            catch (HttpRequestException)
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "Product service is unavailable."));
            }
        }

        private async Task<(CatalogUser? User, IActionResult? Error)> GetUser(int userId)
        {
            var userServiceUrl = _configuration["Services:UserServiceUrl"];
            if (string.IsNullOrWhiteSpace(userServiceUrl))
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "User service URL is not configured."));
            }

            try
            {
                var response = await _httpClientFactory.CreateClient().GetAsync(
                    $"{userServiceUrl.TrimEnd('/')}/api/Users/{userId}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (null, null);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return (null, StatusCode((int)HttpStatusCode.BadGateway, "User service could not validate the user."));
                }

                return (await response.Content.ReadFromJsonAsync<CatalogUser>(), null);
            }
            catch (HttpRequestException)
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "User service is unavailable."));
            }
        }

        private async Task<(CreatedPayment? Payment, IActionResult? Error)> CreatePayment(
            int orderId,
            decimal amount,
            string paymentMethod)
        {
            var paymentServiceUrl = _configuration["Services:PaymentServiceUrl"];
            if (string.IsNullOrWhiteSpace(paymentServiceUrl))
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "Payment service URL is not configured."));
            }

            try
            {
                var response = await _httpClientFactory.CreateClient().PostAsJsonAsync(
                    $"{paymentServiceUrl.TrimEnd('/')}/api/Payments",
                    new CreatePaymentRequest(orderId, amount, paymentMethod));
                if (!response.IsSuccessStatusCode)
                {
                    return (null, StatusCode((int)HttpStatusCode.BadGateway, "Payment service could not process the payment."));
                }

                return (await response.Content.ReadFromJsonAsync<CreatedPayment>(), null);
            }
            catch (HttpRequestException)
            {
                return (null, StatusCode((int)HttpStatusCode.ServiceUnavailable, "Payment service is unavailable."));
            }
        }
    }

    public record CreateOrderRequest(int UserId, int ProductId, int Quantity);
    public record UpdateOrderRequest(int ProductId, int Quantity);
    public record UpdateOrderStatusRequest(string Status);
    public record CheckoutRequest(int UserId, int ProductId, int Quantity, string PaymentMethod);
    public record CatalogProduct(int Id, string Name, string Description, decimal Price, int Stock);
    public record CatalogUser(int Id, string Name, string Email);
    public record CreatePaymentRequest(int OrderId, decimal Amount, string PaymentMethod);
    public record CreatedPayment(int Id, int OrderId, decimal Amount, string PaymentMethod, string Status, DateTime CreatedAtUtc, DateTime? UpdatedAtUtc, DateTime? RefundedAtUtc);
    public record CheckoutResponse(Order Order, CreatedPayment Payment);
}