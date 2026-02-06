using RegistrationEventService.Application.Abstractions;
using RegistrationEventService.Application.DTOs;
using RegistrationEventService.Domain.Abstractions;
using RegistrationEventService.Domain.Entities;
using RegistrationEventService.Domain.Events;
using RegistrationEventService.Domain.Exceptions;

namespace RegistrationEventService.Application.Services;

/// <summary>
/// Implements user registration workflows.
/// Coordinates persistence and event publication while keeping domain rules intact.
/// </summary>
public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate email
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            throw new UserAlreadyExistsException(request.Email);
        }

        // Create and persist the user
        var user = User.Create(request.Name, request.Email);
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event (fire-and-forget to SNS)
        var @event = new UserCreatedEvent(user.Id, user.Email, user.CreatedAt);
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        return MapToResponse(user);
    }

    /// <inheritdoc />
    public async Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // check if user exists
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(id);
        }

        return MapToResponse(user);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        return users.Select(MapToResponse).ToList();
    }

    private static UserResponse MapToResponse(User user) =>
        new(user.Id, user.Name, user.Email, user.CreatedAt);
}
