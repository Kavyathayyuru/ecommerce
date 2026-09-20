using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Moq;
using Xunit;
public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrder_WithValidData_ReturnsOrder()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var product = new Product
        {
            Id = 1,
            Name = "Dell Laptop",
            Description = "Business Laptop",
            Price = 70000,
            Stock = 30
        };

        mockProductRepository
            .Setup(repo => repo.GetProductById(1))
            .ReturnsAsync(product);

        mockProductRepository
            .Setup(repo => repo.UpdateProduct(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        mockOrderRepository
            .Setup(repo => repo.CreateOrder(It.IsAny<Order>()))
            .ReturnsAsync((Order order) => order);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 1,
                Quantity = 2
            }
        }
        };

        // Act
        var result = await service.CreateOrder(1, createOrderDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.UserId);
        Assert.Equal(140000, result.TotalAmount);
        Assert.Equal("Pending", result.Status);

        Assert.Single(result.OrderItems);

        Assert.Equal(1, result.OrderItems[0].ProductId);
        Assert.Equal(2, result.OrderItems[0].Quantity);
        Assert.Equal(70000, result.OrderItems[0].UnitPrice);

        Assert.Equal(28, product.Stock);

        mockProductRepository.Verify(
            repo => repo.GetProductById(1),
            Times.Once);

        mockProductRepository.Verify(
            repo => repo.UpdateProduct(product),
            Times.Once);

        mockOrderRepository.Verify(
            repo => repo.CreateOrder(It.IsAny<Order>()),
            Times.Once);
    }
    [Fact]
    public async Task CreateOrder_WhenItemsAreEmpty_ReturnsNull()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>()
        };

        // Act
        var result = await service.CreateOrder(1, createOrderDto);

        // Assert
        Assert.Null(result);

        mockProductRepository.Verify(
            repo => repo.GetProductById(It.IsAny<int>()),
            Times.Never);

        mockOrderRepository.Verify(
            repo => repo.CreateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task CreateOrder_WhenProductDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        mockProductRepository
            .Setup(repo => repo.GetProductById(999))
            .ReturnsAsync((Product?)null);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 999,
                Quantity = 1
            }
        }
        };

        // Act
        var result = await service.CreateOrder(1, createOrderDto);

        // Assert
        Assert.Null(result);

        mockProductRepository.Verify(
            repo => repo.GetProductById(999),
            Times.Once);

        mockProductRepository.Verify(
            repo => repo.UpdateProduct(It.IsAny<Product>()),
            Times.Never);

        mockOrderRepository.Verify(
            repo => repo.CreateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task CreateOrder_WhenQuantityIsInvalid_ReturnsNull()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var product = new Product
        {
            Id = 1,
            Name = "Dell Laptop",
            Description = "Business Laptop",
            Price = 70000,
            Stock = 30
        };

        mockProductRepository
            .Setup(repo => repo.GetProductById(1))
            .ReturnsAsync(product);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 1,
                Quantity = 0
            }
        }
        };

        // Act
        var result = await service.CreateOrder(1, createOrderDto);

        // Assert
        Assert.Null(result);

        mockProductRepository.Verify(
            repo => repo.GetProductById(1),
            Times.Once);

        mockProductRepository.Verify(
            repo => repo.UpdateProduct(It.IsAny<Product>()),
            Times.Never);

        mockOrderRepository.Verify(
            repo => repo.CreateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task CreateOrder_WhenStockIsInsufficient_ReturnsNull()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var product = new Product
        {
            Id = 1,
            Name = "Dell Laptop",
            Description = "Business Laptop",
            Price = 70000,
            Stock = 5
        };

        mockProductRepository
            .Setup(repo => repo.GetProductById(1))
            .ReturnsAsync(product);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        var createOrderDto = new CreateOrderDto
        {
            Items = new List<CreateOrderItemDto>
        {
            new CreateOrderItemDto
            {
                ProductId = 1,
                Quantity = 10
            }
        }
        };

        // Act
        var result = await service.CreateOrder(1, createOrderDto);

        // Assert
        Assert.Null(result);

        mockProductRepository.Verify(
            repo => repo.GetProductById(1),
            Times.Once);

        mockProductRepository.Verify(
            repo => repo.UpdateProduct(It.IsAny<Product>()),
            Times.Never);

        mockOrderRepository.Verify(
            repo => repo.CreateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task GetOrdersByUserId_ReturnsOrders()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var orders = new List<Order>
    {
        new Order
        {
            Id = 1,
            UserId = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 140000,
            Status = "Pending"
        },
        new Order
        {
            Id = 2,
            UserId = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 70000,
            Status = "Paid"
        }
    };

        mockOrderRepository
            .Setup(repo => repo.GetOrdersByUserId(1))
            .ReturnsAsync(orders);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        // Act
        var result = await service.GetOrdersByUserId(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal(140000, result[0].TotalAmount);

        Assert.Equal(2, result[1].Id);
        Assert.Equal(70000, result[1].TotalAmount);

        mockOrderRepository.Verify(
            repo => repo.GetOrdersByUserId(1),
            Times.Once);
    }
    [Fact]
    public async Task GetOrderById_WhenOrderExists_ReturnsOrder()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            OrderDate = DateTime.UtcNow,
            TotalAmount = 140000,
            Status = "Pending"
        };

        mockOrderRepository
            .Setup(repo => repo.GetOrderById(1))
            .ReturnsAsync(order);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        // Act
        var result = await service.GetOrderById(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, result.UserId);
        Assert.Equal(140000, result.TotalAmount);
        Assert.Equal("Pending", result.Status);

        mockOrderRepository.Verify(
            repo => repo.GetOrderById(1),
            Times.Once);
    }
    [Fact]
    public async Task GetOrderById_WhenOrderDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockOrderRepository = new Mock<IOrderRepository>();
        var mockProductRepository = new Mock<IProductRepository>();

        mockOrderRepository
            .Setup(repo => repo.GetOrderById(999))
            .ReturnsAsync((Order?)null);

        var service = new OrderService(
            mockOrderRepository.Object,
            mockProductRepository.Object);

        // Act
        var result = await service.GetOrderById(999);

        // Assert
        Assert.Null(result);

        mockOrderRepository.Verify(
            repo => repo.GetOrderById(999),
            Times.Once);
    }

}
