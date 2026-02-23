using FluentAssertions;
using Moq;
using RegistrationEventService.Application.DTOs;
using RegistrationEventService.Application.Services;
using RegistrationEventService.Domain.Abstractions;
using RegistrationEventService.Domain.Entities;
using RegistrationEventService.Domain.Events;
using RegistrationEventService.Domain.Exceptions;
using Xunit;

namespace RegistrationEventService.UnitTests.Application;

/// <summary>
/// Unit tests for the <see cref="ProductService"/> class.
/// </summary>

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepositoryMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _sut = new ProductService(
            _productRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_ShouldCreateProductAndPublishEvent()
    {
        // Arrange
        var request = new CreateProductRequest("Test Product", "TESTSKU", "Test Supplier", 9.99m, "Test Description");

        _productRepositoryMock
            .Setup(r => r.GetBySkuAsync(request.Sku, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateProductAsync(request);

        // Assert
        result.Name.Should().Be(request.Name);
        result.Sku.Should().Be(request.Sku);
        result.Supplier.Should().Be(request.Supplier);
        result.Price.Should().Be(request.Price);
        result.Description.Should().Be(request.Description);

        _productRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Product>(p =>
                p.Name == request.Name &&
                p.Sku == request.Sku &&
                p.Supplier == request.Supplier &&
                p.Price == request.Price &&
                p.Description == request.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<ProductCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithDuplicateSku_ShouldReturnExistingProduct()
    {
        // Arrange
        var request = new CreateProductRequest("Test Product", "TESTSKU", "Test Supplier", 9.99m, "Test Description");
        var existingProduct = Product.Create("Test Product", "TESTSKU", "Test Supplier", 9.99m, "Test Description");

        _productRepositoryMock
            .Setup(r => r.GetBySkuAsync("TESTSKU", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);

        // Act
        var act = () => _sut.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ProductAlreadyExistsException>()
            .WithMessage($"*{request.Sku}*");

        _productRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var product = Product.Create("Test Product", "TESTSKU", "Test Supplier", 9.99m, "Test Description");

        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetProductByIdAsync(product.Id);

        // Assert
        result.Name.Should().Be(product.Name);
        result.Sku.Should().Be(product.Sku);
        result.Supplier.Should().Be(product.Supplier);
        result.Price.Should().Be(product.Price);
        result.Description.Should().Be(product.Description);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithNonExistingProduct_ShouldThrowProductNotFoundException()
    {
        // Arrange
        _productRepositoryMock
            .Setup(r => r.GetByIdAsync(2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _sut.GetProductByIdAsync(2026);

        // Assert
        await act.Should().ThrowAsync<ProductNotFoundException>()
            .WithMessage($"*2026*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetAllProductsAsync_ShouldReturnCorrectNumberOfProducts(int count)
    {
        // Arrange
        var products = Enumerable.Range(1, count)
            .Select(i => Product.Create($"Product {i}", $"SKU{i}", $"Supplier {i}", 9.99m + i, $"Description {i}"))
            .ToList();

        _productRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        // Act
        var result = await _sut.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(count);
        result.Select(p => p.Name).Should()
            .BeEquivalentTo(Enumerable.Range(1, count).Select(i => $"Product {i}"));
        result.Select(p => p.Sku).Should()
            .BeEquivalentTo(Enumerable.Range(1, count).Select(i => $"SKU{i}"));
    }
}