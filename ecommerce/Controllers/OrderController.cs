using ECommerce.Business.Services;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
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