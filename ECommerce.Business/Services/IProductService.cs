using ECommerce.Models;
using ECommerce.Models.DTOs;

namespace ECommerce.Business.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts();

        Task<Product?> GetProductById(int id);

        Task<Product> AddProduct(CreateProductDto productDto);

        Task<Product?> UpdateProduct(int id, UpdateProductDto productDto);

        Task<bool> DeleteProduct(int id);
    }
}