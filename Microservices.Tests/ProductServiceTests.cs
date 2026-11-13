using Microsoft.Extensions.Caching.Memory;
using ProductService.Models;
using ProductService.Repositories;
using ProductService.Services;
using Xunit;

namespace Microservices.Tests;

public sealed class ProductServiceTests
{
    [Fact]
    public async Task CreateAsyncRejectsNegativeStock()
    {
        var repository = new FakeProductRepository();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductService.Services.ProductService(repository, cache);

        var (product, error) = await service.CreateAsync(new Product { Name = "Item", Price = 1, Stock = -1 }, CancellationToken.None);

        Assert.Null(product);
        Assert.NotNull(error);
        Assert.Single(repository.Products);
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public List<Product> Products { get; } = [new Product { Id = 1, Name = "Initial", Price = 1, Stock = 1 }];

        public Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<Product>>(Products.ToArray());
        public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) => Task.FromResult(Products.FirstOrDefault(product => product.Id == id));
        public Task<Product> AddAsync(Product product, CancellationToken cancellationToken) { product.Id = Products.Max(item => item.Id) + 1; Products.Add(product); return Task.FromResult(product); }
        public Task<Product?> UpdateAsync(int id, Product updatedProduct, CancellationToken cancellationToken) => Task.FromResult<Product?>(null);
        public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken) => Task.FromResult(false);
    }
}
