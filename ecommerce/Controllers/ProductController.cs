using Microsoft.AspNetCore.Authorization;
using ECommerce.Business.Services;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var products = await _productService.GetProducts();

            return Ok(products);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            var product = await _productService.GetProductById(id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> AddProduct(
            CreateProductDto productDto)
        {
            var product = await _productService.AddProduct(productDto);

            return Ok(product);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Product>> UpdateProduct(
            int id,
            [FromBody] UpdateProductDto productDto)
        {
            var product = await _productService.UpdateProduct(id, productDto);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var deleted = await _productService.DeleteProduct(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        [HttpDelete("bulk-delete")]
        public async Task<IActionResult> DeleteProducts(BulkDeleteProductsDto dto)
        {
            if (dto.ProductIds == null || dto.ProductIds.Count == 0)
                return BadRequest("Please provide at least one product ID.");

            var result = await _productService.DeleteProducts(dto.ProductIds);

            if (!result)
                return BadRequest("Unable to delete products.");

            return Ok(new
            {
                message = "Products deleted successfully.",
                deletedCount = dto.ProductIds.Count
            });
        }
        [HttpPost("bulk-upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Please upload an Excel file.");
            }

            if (Path.GetExtension(file.FileName).ToLowerInvariant() != ".xlsx")
            {
                return BadRequest("Only .xlsx Excel files are supported.");
            }

            await using var stream = file.OpenReadStream();

            var result = await _productService.BulkAddProducts(stream);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}