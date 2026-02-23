using FluentAssertions;
using RegistrationEventService.Domain.Entities;
using Xunit;

namespace RegistrationEventService.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="Product"/> entity.
/// </summary>
public class ProductTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnProductWithCorrectProperties()
    {
        // Arrange
        var name = "Test Product";
        var sku = "TESTSKU";
        var supplier = "Test Supplier";
        var price = 9.99m;
        var description = "Test Description";

        // Act
        var product = Product.Create(name, sku, supplier, price, description);

        // Assert
        product.Name.Should().Be(name);
        product.Sku.Should().Be(sku);
        product.Supplier.Should().Be(supplier);
        product.Price.Should().Be(price);
        product.Description.Should().Be(description);
        product.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        product.Id.Should().Be(0); // Not yet persisted
    }

    [Fact]
    public void Create_WithNullDescription_ShouldDefaultToEmptyString()
    {
        // Act
        var product = Product.Create("Product", "SKU", "Supplier", 10m, null);

        // Assert
        product.Description.Should().Be(string.Empty);
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var product = Product.Create("Test", "SKU", "Supplier", 5m, "Desc");

        // Assert
        var after = DateTime.UtcNow;
        product.CreatedAt.Should().BeOnOrAfter(before);
        product.CreatedAt.Should().BeOnOrBefore(after);
    }
}
