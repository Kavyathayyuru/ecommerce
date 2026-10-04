using Microsoft.AspNetCore.Mvc;
using ProductService.Models;
using ProductService.Data;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;

        public ProductsController(ProductDbContext context)
        {
            _context = context;
        }


        // GET: api/Products
        [HttpGet]
        public IActionResult GetProducts()
        {
            var products = _context.Products.ToList();

            return Ok(products);
        }


        // GET: api/Products/1
        [HttpGet("{id}")]
        public IActionResult GetProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }


        // POST: api/Products
        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name) || product.Price < 0 || product.Stock < 0)
            {
                return BadRequest("Name is required and price and stock cannot be negative.");
            }

            product.Id = 0;
            _context.Products.Add(product);
            _context.SaveChanges();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product
            );
        }


        // PUT: api/Products/1
        [HttpPut("{id}")]
        public IActionResult UpdateProduct(int id, Product updatedProduct)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(updatedProduct.Name) || updatedProduct.Price < 0 || updatedProduct.Stock < 0)
            {
                return BadRequest("Name is required and price and stock cannot be negative.");
            }

            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;

            _context.SaveChanges();

            return Ok(product);
        }


        // DELETE: api/Products/1
        [HttpDelete("{id}")]
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            _context.SaveChanges();

            return NoContent();
        }
    }
}