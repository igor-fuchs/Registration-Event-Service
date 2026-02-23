using FluentAssertions;
using RegistrationEventService.Domain.Entities;
using Xunit;

namespace RegistrationEventService.UnitTests.Domain;

/// <summary>
/// Unit tests for the <see cref="User"/> entity.
/// </summary>
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnUserWithCorrectProperties()
    {
        // Arrange
        var name = "John Doe";
        var email = "john.doe@example.com";

        // Act
        var user = User.Create(name, email);

        // Assert
        user.Name.Should().Be(name);
        user.Email.Should().Be(email);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.Id.Should().Be(0); // Not yet persisted
    }

    [Fact]
    public void Create_ShouldSetCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var user = User.Create("Test", "test@example.com");

        // Assert
        var after = DateTime.UtcNow;
        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }
}
