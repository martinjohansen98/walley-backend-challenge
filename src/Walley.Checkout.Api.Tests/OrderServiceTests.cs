using FluentAssertions;
using Xunit;
using Walley.Checkout.Api.Models;
using Walley.Checkout.Api.Services;

namespace Walley.Checkout.Api.Tests;

public class OrderServiceTests
{
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService();
    }

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllSeededOrders()
    {
        // Act
        var result = await _sut.GetAllOrdersAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithExistingId_ShouldReturnMatchingOrder()
    {
        // Arrange
        var existingId = "ORD-001";

        // Act
        var result = await _sut.GetOrderByIdAsync(existingId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task GetOrderByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = "ORD-DOES-NOT-EXIST";

        // Act
        var result = await _sut.GetOrderByIdAsync(nonExistingId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldAssignGeneratedOrderId()
    {
        // Arrange
        var order = BuildTestOrder();

        // Act
        var result = await _sut.CreateOrderAsync(order);

        // Assert
        result.Id.Should().NotBeNullOrWhiteSpace();
        result.Id.Should().StartWith("ORD-");
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldSetStatusToPending()
    {
        // Arrange
        var order = BuildTestOrder();

        // Act
        var result = await _sut.CreateOrderAsync(order);

        // Assert
        result.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldCalculateTotalFromLines()
    {
        // Arrange
        var order = BuildTestOrder();

        // Act
        var result = await _sut.CreateOrderAsync(order);

        // Assert
        result.TotalAmount.Should().Be(300.00m);
    }

    [Fact]
    public async Task CreateOrderAsync_ShouldMakeOrderRetrievableById()
    {
        // Arrange
        var order = BuildTestOrder();

        // Act
        var created = await _sut.CreateOrderAsync(order);
        var retrieved = await _sut.GetOrderByIdAsync(created.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(created.Id);
    }

    private static Order BuildTestOrder() => new()
    {
        CustomerName = "Ers Majonas",
        CustomerEmail = "ers@majonas.com",
        Currency = "SEK",
        Lines = new List<OrderLine>
        {
            new() { ProductName = "Majonas", Quantity = 2, UnitPrice = 100.00m },
            new() { ProductName = "Butter", Quantity = 1, UnitPrice = 100.00m }
        }
    };
}
