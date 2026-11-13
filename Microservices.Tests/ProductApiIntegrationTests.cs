using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using ProductService.Data;
using Xunit;

namespace Microservices.Tests;

public sealed class ProductApiIntegrationTests : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient _client;

    public ProductApiIntegrationTests(ProductApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task ProductReadEndpointIsAvailableWithoutAuthentication()
    {
        var response = await _client.GetAsync("/api/Products");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

public sealed class ProductApiFactory : WebApplicationFactory<global::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ProductDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProductDbContext>>();
            services.AddDbContext<ProductDbContext>(options => options.UseInMemoryDatabase("ProductApiIntegrationTests"));
        });
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["Authentication:SigningKey"] = "integration-test-signing-key-must-be-at-least-32-bytes",
                ["Authentication:Issuer"] = "ECommerceMicroservices",
                ["Authentication:Audience"] = "ECommerceMicroservices"
            }));
    }
}
