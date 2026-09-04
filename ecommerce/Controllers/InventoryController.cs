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
    }
}