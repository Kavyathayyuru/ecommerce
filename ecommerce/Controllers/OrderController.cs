using ECommerce.Business.Services;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IConfiguration _configuration;

        public OrderController(
            IOrderService orderService,
            IConfiguration configuration)
        {
            _orderService = orderService;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder(
            CreateOrderDto createOrderDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var order = await _orderService.CreateOrder(
                userId,
                createOrderDto);

            if (order == null)
                return BadRequest(
                    "Unable to create order. Check product and stock.");

            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var orders = await _orderService
                .GetOrdersByUserId(userId);

            return Ok(orders);
        }

        [HttpGet("admin/all")]
        public async Task<IActionResult> GetAllOrdersForAdmin()
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var adminEmails = _configuration
                .GetSection("Admin:Emails")
                .GetChildren()
                .Select(section => section.Value)
                .Where(value => !string.IsNullOrWhiteSpace(value));

            if (email == null || !adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase))
                return Forbid();

            var orders = await _orderService.GetAllOrders();

            var result = orders.Select(order => new
            {
                order.Id,
                order.OrderDate,
                order.Status,
                order.TotalAmount,
                Customer = order.User == null ? null : new
                {
                    order.User.Id,
                    order.User.Username,
                    order.User.Email
                },
                Items = order.OrderItems.Select(item => new
                {
                    item.ProductId,
                    ProductName = item.Product?.Name,
                    item.Quantity,
                    item.UnitPrice
                })
            });

            return Ok(result);
        }

        [HttpPatch("admin/{orderId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(
            int orderId,
            UpdateOrderStatusDto updateOrderStatusDto)
        {
            var order = await _orderService.UpdateOrderStatus(
                orderId,
                updateOrderStatusDto.Status);

            return order == null
                ? BadRequest("Order not found or status is invalid.")
                : Ok(order);
        }
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var order = await _orderService
                .GetOrderById(orderId);

            if (order == null)
                return NotFound("Order not found.");

            if (order.UserId != userId)
                return Forbid();

            return Ok(order);
        }
        [HttpPost("{orderId}/checkout")]
        public async Task<IActionResult> CheckoutOrder(int orderId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var success = await _orderService
                .CheckoutOrder(orderId, userId);

            if (!success)
                return BadRequest("Unable to checkout the order.");

            return Ok("Order checked out successfully.");
        }
    }
}