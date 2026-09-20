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
        Task AddProducts(List<Product> products);

        Task DeleteProducts(List<int> productIds);
    }
}
