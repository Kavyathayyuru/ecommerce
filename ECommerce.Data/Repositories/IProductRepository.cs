using ECommerce.Models;

namespace ECommerce.Data.Repositories
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProducts();

        Task<Product?> GetProductById(int id);

        Task AddProduct(Product product);

        Task UpdateProduct(Product product);

        Task DeleteProduct(int id);
    }
}
