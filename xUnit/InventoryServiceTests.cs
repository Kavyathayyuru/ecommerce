using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using Moq;
using Xunit;

namespace xUnit
{
    public class InventoryServiceTests
    {
        [Fact]
        public async Task GetAllInventory_ReturnsProducts()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var expectedProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Dell Laptop",
                    Description = "Business Laptop",
                    Price = 70000,
                    Stock = 20
                },
                new Product
                {
                    Id = 2,
                    Name = "HP Laptop",
                    Description = "Student Laptop",
                    Price = 55000,
                    Stock = 15
                }
            };

            mockRepository
                .Setup(repo => repo.GetAllInventory())
                .ReturnsAsync(expectedProducts);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.GetAllInventory();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            Assert.Equal("Dell Laptop", result[0].Name);
            Assert.Equal(70000, result[0].Price);
            Assert.Equal(20, result[0].Stock);

            Assert.Equal("HP Laptop", result[1].Name);
            Assert.Equal(55000, result[1].Price);
            Assert.Equal(15, result[1].Stock);

            mockRepository.Verify(
                repo => repo.GetAllInventory(),
                Times.Once);
        }
        [Fact]
        public async Task UpdateStock_WhenProductExistsAndStockIsValid_ReturnsTrue()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var product = new Product
            {
                Id = 1,
                Name = "Dell Laptop",
                Description = "Business Laptop",
                Price = 70000,
                Stock = 10
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(product);

            mockRepository
                .Setup(repo => repo.UpdateStock(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.UpdateStock(1, 25);

            // Assert
            Assert.True(result);
            Assert.Equal(25, product.Stock);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.UpdateStock(It.IsAny<Product>()),
                Times.Once);
        }
        [Fact]
        public async Task UpdateStock_WhenProductDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            mockRepository
                .Setup(repo => repo.GetProductById(999))
                .ReturnsAsync((Product?)null);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.UpdateStock(999, 25);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(999),
                Times.Once);

            mockRepository.Verify(
                repo => repo.UpdateStock(It.IsAny<Product>()),
                Times.Never);
        }
        [Fact]
        public async Task UpdateStock_WhenStockIsNegative_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.UpdateStock(1, -10);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(It.IsAny<int>()),
                Times.Never);

            mockRepository.Verify(
                repo => repo.UpdateStock(It.IsAny<Product>()),
                Times.Never);
        }
        [Fact]
        public async Task IncreaseStock_WhenProductExistsAndQuantityIsValid_ReturnsTrue()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var product = new Product
            {
                Id = 1,
                Name = "Dell Laptop",
                Description = "Business Laptop",
                Price = 70000,
                Stock = 10
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(product);

            mockRepository
                .Setup(repo => repo.IncreaseStock(It.IsAny<Product>(), 5))
                .Returns(Task.CompletedTask);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.IncreaseStock(1, 5);

            // Assert
            Assert.True(result);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.IncreaseStock(product, 5),
                Times.Once);
        }
        [Fact]
        public async Task IncreaseStock_WhenQuantityIsInvalid_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.IncreaseStock(1, 0);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(It.IsAny<int>()),
                Times.Never);

            mockRepository.Verify(
                repo => repo.IncreaseStock(
                    It.IsAny<Product>(),
                    It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task IncreaseStock_WhenProductDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            mockRepository
                .Setup(repo => repo.GetProductById(999))
                .ReturnsAsync((Product?)null);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.IncreaseStock(999, 5);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(999),
                Times.Once);

            mockRepository.Verify(
                repo => repo.IncreaseStock(
                    It.IsAny<Product>(),
                    It.IsAny<int>()),
                Times.Never);
        }
        [Fact]
        public async Task DecreaseStock_WhenProductExistsAndQuantityIsValid_ReturnsTrue()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var product = new Product
            {
                Id = 1,
                Name = "Dell Laptop",
                Description = "Business Laptop",
                Price = 70000,
                Stock = 20
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(product);

            mockRepository
                .Setup(repo => repo.DecreaseStock(It.IsAny<Product>(), 5))
                .Returns(Task.CompletedTask);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.DecreaseStock(1, 5);

            // Assert
            Assert.True(result);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.DecreaseStock(product, 5),
                Times.Once);
        }
        [Fact]
        public async Task DecreaseStock_WhenQuantityExceedsAvailableStock_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IInventoryRepository>();

            var product = new Product
            {
                Id = 1,
                Name = "Dell Laptop",
                Description = "Business Laptop",
                Price = 70000,
                Stock = 10
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(product);

            var service = new InventoryService(mockRepository.Object);

            // Act
            var result = await service.DecreaseStock(1, 15);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.DecreaseStock(It.IsAny<Product>(), It.IsAny<int>()),
                Times.Never);
        }

    }
}