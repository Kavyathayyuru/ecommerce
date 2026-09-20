using ECommerce.Business.Services;
using ECommerce.Data.Repositories;
using ECommerce.Models;
using ECommerce.Models.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

public class AuthServiceTests
{
    [Fact]
    public async Task Register_WithValidData_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockRepository
            .Setup(repo => repo.GetUserByEmail("test@gmail.com"))
            .ReturnsAsync((User?)null);

        mockPasswordHasher
            .Setup(hasher => hasher.HashPassword(
                It.IsAny<User>(),
                "Password123"))
            .Returns("hashedPassword");

        mockRepository
            .Setup(repo => repo.AddUser(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        var service = new AuthService(
            mockRepository.Object,
            mockPasswordHasher.Object,
            mockConfiguration.Object);

        var registerDto = new RegisterUserDto
        {
            Username = "testuser",
            Email = "test@gmail.com",
            Password = "Password123"
        };

        // Act
        var result = await service.RegisterUser(registerDto);

        // Assert
        Assert.True(result);

        mockRepository.Verify(
            repo => repo.GetUserByEmail("test@gmail.com"),
            Times.Once);

        mockPasswordHasher.Verify(
            hasher => hasher.HashPassword(
                It.IsAny<User>(),
                "Password123"),
            Times.Once);

        mockRepository.Verify(
            repo => repo.AddUser(It.IsAny<User>()),
            Times.Once);
    }
    [Fact]
    public async Task RegisterUser_WhenEmailAlreadyExists_ReturnsFalse()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockConfiguration = new Mock<IConfiguration>();

        var existingUser = new User
        {
            Id = 1,
            Username = "existinguser",
            Email = "test@gmail.com",
            PasswordHash = "existingHash"
        };

        mockRepository
            .Setup(repo => repo.GetUserByEmail("test@gmail.com"))
            .ReturnsAsync(existingUser);

        var service = new AuthService(
            mockRepository.Object,
            mockPasswordHasher.Object,
            mockConfiguration.Object);

        var registerDto = new RegisterUserDto
        {
            Username = "newuser",
            Email = "test@gmail.com",
            Password = "Password123"
        };

        // Act
        var result = await service.RegisterUser(registerDto);

        // Assert
        Assert.False(result);

        mockRepository.Verify(
            repo => repo.GetUserByEmail("test@gmail.com"),
            Times.Once);

        mockPasswordHasher.Verify(
            hasher => hasher.HashPassword(
                It.IsAny<User>(),
                It.IsAny<string>()),
            Times.Never);

        mockRepository.Verify(
            repo => repo.AddUser(It.IsAny<User>()),
            Times.Never);
    }
    [Fact]
    public async Task Login_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockConfiguration = new Mock<IConfiguration>();

        mockRepository
            .Setup(repo => repo.GetUserByEmail("unknown@gmail.com"))
            .ReturnsAsync((User?)null);

        var service = new AuthService(
            mockRepository.Object,
            mockPasswordHasher.Object,
            mockConfiguration.Object);

        var loginDto = new LoginDto
        {
            Email = "unknown@gmail.com",
            Password = "Password123"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.Null(result);

        mockRepository.Verify(
            repo => repo.GetUserByEmail("unknown@gmail.com"),
            Times.Once);

        mockPasswordHasher.Verify(
            hasher => hasher.VerifyHashedPassword(
                It.IsAny<User>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Never);
    }
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockConfiguration = new Mock<IConfiguration>();

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@gmail.com",
            PasswordHash = "hashedPassword"
        };

        mockRepository
            .Setup(repo => repo.GetUserByEmail("test@gmail.com"))
            .ReturnsAsync(user);

        mockPasswordHasher
            .Setup(hasher => hasher.VerifyHashedPassword(
                user,
                "hashedPassword",
                "Password123"))
            .Returns(PasswordVerificationResult.Success);

        mockConfiguration
            .Setup(config => config["Jwt:Key"])
            .Returns("ThisIsMySuperSecretKey1234567890");

        mockConfiguration
            .Setup(config => config["Jwt:Issuer"])
            .Returns("ECommerceAPI");

        mockConfiguration
            .Setup(config => config["Jwt:Audience"])
            .Returns("ECommerceClient");

        mockConfiguration
            .Setup(config => config["Jwt:ExpiryMinutes"])
            .Returns("60");

        var service = new AuthService(
            mockRepository.Object,
            mockPasswordHasher.Object,
            mockConfiguration.Object);

        var loginDto = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "Password123"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);

        mockRepository.Verify(
            repo => repo.GetUserByEmail("test@gmail.com"),
            Times.Once);

        mockPasswordHasher.Verify(
            hasher => hasher.VerifyHashedPassword(
                user,
                "hashedPassword",
                "Password123"),
            Times.Once);
    }
    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsNull()
    {
        // Arrange
        var mockRepository = new Mock<IUserRepository>();
        var mockPasswordHasher = new Mock<IPasswordHasher<User>>();
        var mockConfiguration = new Mock<IConfiguration>();

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@gmail.com",
            PasswordHash = "hashedPassword"
        };

        mockRepository
            .Setup(repo => repo.GetUserByEmail("test@gmail.com"))
            .ReturnsAsync(user);

        mockPasswordHasher
            .Setup(hasher => hasher.VerifyHashedPassword(
                user,
                "hashedPassword",
                "WrongPassword"))
            .Returns(PasswordVerificationResult.Failed);

        var service = new AuthService(
            mockRepository.Object,
            mockPasswordHasher.Object,
            mockConfiguration.Object);

        var loginDto = new LoginDto
        {
            Email = "test@gmail.com",
            Password = "WrongPassword"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.Null(result);

        mockRepository.Verify(
            repo => repo.GetUserByEmail("test@gmail.com"),
            Times.Once);

        mockPasswordHasher.Verify(
            hasher => hasher.VerifyHashedPassword(
                user,
                "hashedPassword",
                "WrongPassword"),
            Times.Once);
    }
}