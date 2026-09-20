using ECommerce.Business.Services;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateStock(
            int productId,
            UpdateStockDto updateStockDto)
        {
            var result = await _inventoryService.UpdateStock(
                productId,
                updateStockDto.Stock);

            if (!result)
                return BadRequest("Product not found or invalid stock.");

            return Ok("Stock updated successfully.");
        }
        [HttpGet]
        public async Task<IActionResult> GetAllInventory()
        {
            var inventory = await _inventoryService.GetAllInventory();

            return Ok(inventory);
        }
        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts(int threshold = 10)
        {
            var products = await _inventoryService.GetLowStockProducts(threshold);

            return Ok(products);
        }
        [HttpPost("{productId}/increase")]
        public async Task<IActionResult> IncreaseStock(int productId,int quantity)
        {
            var result = await _inventoryService
                .IncreaseStock(productId, quantity);

            if (!result)
            {
                return BadRequest(
                    "Product not found or invalid quantity.");
            }

            return Ok("Stock increased successfully.");
        }
        [HttpPost("{productId}/decrease")]
        public async Task<IActionResult> DecreaseStock(
    int productId,
    int quantity)
        {
            var result = await _inventoryService
                .DecreaseStock(productId, quantity);

            if (!result)
            {
                return BadRequest(
                    "Product not found, invalid quantity, or insufficient stock.");
            }

            return Ok("Stock decreased successfully.");
        }
    }
}