using Microsoft.Extensions.Caching.Memory;
using ProductService.Models;
using ProductService.Repositories;
using Xunit;

namespace Microservices.Tests;

public sealed class ProductServiceCacheTests
{
    [Fact]
    public async Task GetAllAsyncCachesResultsUntilCatalogChanges()
    {
        var repository = new FakeProductRepository();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new ProductService.Services.ProductService(repository, cache);

        var firstRead = await service.GetAllAsync(CancellationToken.None);
        repository.Products.Add(new Product { Id = 2, Name = "Later", Price = 2, Stock = 2 });
        var cachedRead = await service.GetAllAsync(CancellationToken.None);

        Assert.Single(firstRead);
        Assert.Single(cachedRead);

        var (created, error) = await service.CreateAsync(new Product { Name = "New", Price = 3, Stock = 4 }, CancellationToken.None);
        var refreshedRead = await service.GetAllAsync(CancellationToken.None);

        Assert.Null(error);
        Assert.NotNull(created);
        Assert.Equal(3, refreshedRead.Count);
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
