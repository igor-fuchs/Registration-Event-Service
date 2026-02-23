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
/// Unit tests for the <see cref="UserService"/> class.
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _sut = new UserService(
            _userRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task CreateUserAsync_WithValidRequest_ShouldCreateUserAndPublishEvent()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "john.doe@example.com");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _unitOfWorkMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateUserAsync(request);

        // Assert
        result.Name.Should().Be(request.Name);
        result.Email.Should().Be(request.Email);

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u => u.Name == request.Name && u.Email == request.Email),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_WithDuplicateEmail_ShouldThrowUserAlreadyExistsException()
    {
        // Arrange
        var request = new CreateUserRequest("John Doe", "existing@example.com");
        var existingUser = User.Create("Existing User", request.Email);

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(request.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        // Act
        var act = () => _sut.CreateUserAsync(request);

        // Assert
        await act.Should().ThrowAsync<UserAlreadyExistsException>()
            .WithMessage($"*{request.Email}*");

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _eventPublisherMock.Verify(
            p => p.PublishAsync(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithExistingUser_ShouldReturnUserResponse()
    {
        // Arrange
        var user = User.Create("John Doe", "john@example.com");
        
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _sut.GetUserByIdAsync(1);

        // Assert
        result.Name.Should().Be(user.Name);
        result.Email.Should().Be(user.Email);
        result.CreatedAt.Should().Be(user.CreatedAt);
    }

    [Fact]
    public async Task GetUserByIdAsync_WithNonExistingUser_ShouldThrowUserNotFoundException()
    {
        // Arrange
        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(2026, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _sut.GetUserByIdAsync(2026);

        // Assert
        await act.Should().ThrowAsync<UserNotFoundException>()
            .WithMessage("*2026*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetAllUsersAsync_ShouldReturnCorrectNumberOfUsers(int count)
    {
        // Arrange
        var users = Enumerable.Range(1, count)
            .Select(i => User.Create($"User {i}", $"user{i}@example.com"))
            .ToList();

        _userRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _sut.GetAllUsersAsync();

        // Assert
        result.Should().HaveCount(count);
        result.Select(u => u.Name).Should()
            .BeEquivalentTo(Enumerable.Range(1, count).Select(i => $"User {i}"));
    }
}
