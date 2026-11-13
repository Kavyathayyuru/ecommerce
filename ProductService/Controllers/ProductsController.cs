using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProductService.Models;
using ProductService.Services;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductService.Services.ProductService _products;

        public ProductsController(ProductService.Services.ProductService products)
        {
            _products = products;
        }


        // GET: api/Products
        [HttpGet]
        public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
        {
            return Ok(await _products.GetAllAsync(cancellationToken));
        }


        // GET: api/Products/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _products.GetByIdAsync(id, cancellationToken);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        // POST: api/Products
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct(Product product, CancellationToken cancellationToken)
        {
            var (created, error) = await _products.CreateAsync(product, cancellationToken);
            if (error is not null)
            {
                return BadRequest(error);
            }

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = created!.Id },
                created
            );
        }


        // PUT: api/Products/1
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, Product updatedProduct, CancellationToken cancellationToken)
        {
            var (product, error) = await _products.UpdateAsync(id, updatedProduct, cancellationToken);
            if (error is not null)
            {
                return BadRequest(error);
            }
            return product is null ? NotFound() : Ok(product);
        }


        // DELETE: api/Products/1
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
        {
            return await _products.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
    }
}