using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using Moq;
using Xunit;

public class PaymentServiceTests
{
    [Fact]
    public async Task ProcessPayment_WhenOrderIsValid_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            Status = "Checkout",
            TotalAmount = 140000
        };

        mockRepository
            .Setup(repo => repo.GetOrderById(1))
            .ReturnsAsync(order);

        mockRepository
            .Setup(repo => repo.UpdateOrder(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        var service = new PaymentService(mockRepository.Object);

        // Act
        var result = await service.ProcessPayment(1, 1);

        // Assert
        Assert.True(result);
        Assert.Equal("Paid", order.Status);

        mockRepository.Verify(
            repo => repo.GetOrderById(1),
            Times.Once);

        mockRepository.Verify(
            repo => repo.UpdateOrder(order),
            Times.Once);
    }
    [Fact]
    public async Task ProcessPayment_WhenOrderDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();

        mockRepository
            .Setup(repo => repo.GetOrderById(999))
            .ReturnsAsync((Order?)null);

        var service = new PaymentService(mockRepository.Object);

        // Act
        var result = await service.ProcessPayment(999, 1);

        // Assert
        Assert.False(result);

        mockRepository.Verify(
            repo => repo.GetOrderById(999),
            Times.Once);

        mockRepository.Verify(
            repo => repo.UpdateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task ProcessPayment_WhenOrderBelongsToAnotherUser_ReturnsFalse()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            Status = "Checkout",
            TotalAmount = 140000
        };

        mockRepository
            .Setup(repo => repo.GetOrderById(1))
            .ReturnsAsync(order);

        var service = new PaymentService(mockRepository.Object);

        // Act
        var result = await service.ProcessPayment(1, 2);

        // Assert
        Assert.False(result);

        Assert.Equal("Checkout", order.Status);

        mockRepository.Verify(
            repo => repo.GetOrderById(1),
            Times.Once);

        mockRepository.Verify(
            repo => repo.UpdateOrder(It.IsAny<Order>()),
            Times.Never);
    }
    [Fact]
    public async Task ProcessPayment_WhenOrderIsNotInCheckoutStatus_ReturnsFalse()
    {
        // Arrange
        var mockRepository = new Mock<IOrderRepository>();

        var order = new Order
        {
            Id = 1,
            UserId = 1,
            Status = "Pending",
            TotalAmount = 140000
        };

        mockRepository
            .Setup(repo => repo.GetOrderById(1))
            .ReturnsAsync(order);

        var service = new PaymentService(mockRepository.Object);

        // Act
        var result = await service.ProcessPayment(1, 1);

        // Assert
        Assert.False(result);

        Assert.Equal("Pending", order.Status);

        mockRepository.Verify(
            repo => repo.GetOrderById(1),
            Times.Once);

        mockRepository.Verify(
            repo => repo.UpdateOrder(It.IsAny<Order>()),
            Times.Never);
    }
}