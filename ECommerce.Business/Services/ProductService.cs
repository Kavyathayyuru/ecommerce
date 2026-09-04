using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> GetProducts()
        {
            return await _productRepository.GetProducts();
        }

        public async Task<Product?> GetProductById(int id)
        {
            return await _productRepository.GetProductById(id);
        }

        public async Task<Product> AddProduct(CreateProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock
            };

            await _productRepository.AddProduct(product);

            return product;
        }

        public async Task<Product?> UpdateProduct(
            int id,
            UpdateProductDto productDto)
        {
            var existingProduct =
                await _productRepository.GetProductById(id);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;

            await _productRepository.UpdateProduct(existingProduct);

            return existingProduct;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var existingProduct =
                await _productRepository.GetProductById(id);

            if (existingProduct == null)
            {
                return false;
            }

            await _productRepository.DeleteProduct(id);

            return true;
        }
    }
}