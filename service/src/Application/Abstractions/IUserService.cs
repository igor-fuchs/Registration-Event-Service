using RegistrationEventService.Application.DTOs;

namespace RegistrationEventService.Application.Abstractions;

/// <summary>
/// Application service contract for user-related operations.
/// Orchestrates domain logic, persistence, and event publishing.
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Creates a new user, persists it, and publishes a UserCreated event.
    /// </summary>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a user by its unique identifier.
    /// </summary>
    Task<UserResponse> GetUserByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all registered users.
    /// </summary>
    Task<IReadOnlyList<UserResponse>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
