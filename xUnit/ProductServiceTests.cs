using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using Moq;
using Xunit;

namespace xUnit
{
    public class ProductServiceTests
    {
        [Fact]
        public async Task GetProductById_WhenProductExists_ReturnsProduct()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            var expectedProduct = new Product
            {
                Id = 1,
                Name = "Test Laptop",
                Description = "Test Description",
                Price = 50000,
                Stock = 10
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(expectedProduct);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.GetProductById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test Laptop", result.Name);
            Assert.Equal(50000, result.Price);
            Assert.Equal(10, result.Stock);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);
        }
    }
}