using Microsoft.Extensions.Caching.Memory;
using ProductService.Models;
using ProductService.Repositories;

namespace ProductService.Services;

public sealed class ProductService(IProductRepository repository, IMemoryCache cache)
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(2);
    private const string ProductListCacheKey = "products:all";

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken) =>
        await cache.GetOrCreateAsync(ProductListCacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;
            return await repository.GetAllAsync(cancellationToken);
        }) ?? [];

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetByIdAsync(id, cancellationToken);

    public async Task<(Product? Product, string? Error)> CreateAsync(Product product, CancellationToken cancellationToken)
    {
        var error = Validate(product);
        if (error is not null) return (null, error);
        product.Id = 0;
        var created = await repository.AddAsync(product, cancellationToken);
        cache.Remove(ProductListCacheKey);
        return (created, null);
    }

    public async Task<(Product? Product, string? Error)> UpdateAsync(int id, Product product, CancellationToken cancellationToken)
    {
        var error = Validate(product);
        if (error is not null) return (null, error);
        var updated = await repository.UpdateAsync(id, product, cancellationToken);
        if (updated is not null) cache.Remove(ProductListCacheKey);
        return (updated, null);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (deleted) cache.Remove(ProductListCacheKey);
        return deleted;
    }

    private static string? Validate(Product product) =>
        string.IsNullOrWhiteSpace(product.Name) || product.Price < 0 || product.Stock < 0
            ? "Name is required and price and stock cannot be negative."
            : null;
}
