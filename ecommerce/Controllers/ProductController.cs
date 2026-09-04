using Microsoft.AspNetCore.Authorization;
using ECommerce.Business.Services;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts()
        {
            var products = await _productService.GetProducts();

            return Ok(products);
        }

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
    }
}