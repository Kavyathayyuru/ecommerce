using ClosedXML.Excel;
using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Moq;
using Xunit;
using System.IO;

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
        [Fact]
        public async Task AddProduct_WithValidData_ReturnsProduct()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            var productDto = new CreateProductDto
            {
                Name = "Lenovo Laptop",
                Description = "Performance Laptop",
                Price = 65000,
                Stock = 20
            };

            mockRepository
                .Setup(repo => repo.AddProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.AddProduct(productDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lenovo Laptop", result.Name);
            Assert.Equal("Performance Laptop", result.Description);
            Assert.Equal(65000, result.Price);
            Assert.Equal(20, result.Stock);

            mockRepository.Verify(
                repo => repo.AddProduct(It.IsAny<Product>()),
                Times.Once);
        }
        [Fact]
        public async Task UpdateProduct_WhenProductExists_ReturnsUpdatedProduct()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            var existingProduct = new Product
            {
                Id = 1,
                Name = "Old Laptop",
                Description = "Old Description",
                Price = 50000,
                Stock = 10
            };

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Laptop",
                Description = "Updated Description",
                Price = 60000,
                Stock = 20
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(existingProduct);

            mockRepository
                .Setup(repo => repo.UpdateProduct(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.UpdateProduct(1, updateDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Updated Laptop", result.Name);
            Assert.Equal("Updated Description", result.Description);
            Assert.Equal(60000, result.Price);
            Assert.Equal(20, result.Stock);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.UpdateProduct(It.IsAny<Product>()),
                Times.Once);
        }
        [Fact]
        public async Task UpdateProduct_WhenProductDoesNotExist_ReturnsNull()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            var updateDto = new UpdateProductDto
            {
                Name = "Updated Laptop",
                Description = "Updated Description",
                Price = 60000,
                Stock = 20
            };

            mockRepository
                .Setup(repo => repo.GetProductById(999))
                .ReturnsAsync((Product?)null);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.UpdateProduct(999, updateDto);

            // Assert
            Assert.Null(result);

            mockRepository.Verify(
                repo => repo.GetProductById(999),
                Times.Once);

            mockRepository.Verify(
                repo => repo.UpdateProduct(It.IsAny<Product>()),
                Times.Never);
        }
        [Fact]
        public async Task DeleteProduct_WhenProductExists_ReturnsTrue()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            var existingProduct = new Product
            {
                Id = 1,
                Name = "Dell Laptop",
                Description = "Business Laptop",
                Price = 70000,
                Stock = 20
            };

            mockRepository
                .Setup(repo => repo.GetProductById(1))
                .ReturnsAsync(existingProduct);

            mockRepository
                .Setup(repo => repo.DeleteProduct(1))
                .Returns(Task.CompletedTask);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.DeleteProduct(1);

            // Assert
            Assert.True(result);

            mockRepository.Verify(
                repo => repo.GetProductById(1),
                Times.Once);

            mockRepository.Verify(
                repo => repo.DeleteProduct(1),
                Times.Once);
        }
        [Fact]
        public async Task DeleteProduct_WhenProductDoesNotExist_ReturnsFalse()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            mockRepository
                .Setup(repo => repo.GetProductById(999))
                .ReturnsAsync((Product?)null);

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.DeleteProduct(999);

            // Assert
            Assert.False(result);

            mockRepository.Verify(
                repo => repo.GetProductById(999),
                Times.Once);

            mockRepository.Verify(
                repo => repo.DeleteProduct(999),
                Times.Never);
        }
        [Fact]
        public async Task BulkAddProducts_With501ValidProducts_CallsRepositoryInTwoBatches()
        {
            // Arrange
            var mockRepository = new Mock<IProductRepository>();

            mockRepository
                .Setup(repo => repo.AddProducts(It.IsAny<List<Product>>()))
                .Returns(Task.CompletedTask);

            var mockStream = new MemoryStream();

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");

                worksheet.Cell(1, 1).Value = "Name";
                worksheet.Cell(1, 2).Value = "Description";
                worksheet.Cell(1, 3).Value = "Price";
                worksheet.Cell(1, 4).Value = "Stock";

                for (int i = 1; i <= 501; i++)
                {
                    int row = i + 1;

                    worksheet.Cell(row, 1).Value = $"Product {i}";
                    worksheet.Cell(row, 2).Value = $"Description {i}";
                    worksheet.Cell(row, 3).Value = 100 + i;
                    worksheet.Cell(row, 4).Value = 10;
                }

                workbook.SaveAs(mockStream);
            }

            mockStream.Position = 0;

            var service = new ProductService(mockRepository.Object);

            // Act
            var result = await service.BulkAddProducts(mockStream);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(501, result.InsertedCount);
            Assert.Empty(result.Errors);

            mockRepository.Verify(
                repo => repo.AddProducts(It.Is<List<Product>>(p => p.Count == 500)),
                Times.Once);

            mockRepository.Verify(
                repo => repo.AddProducts(It.Is<List<Product>>(p => p.Count == 1)),
                Times.Once);
        }

    }
}