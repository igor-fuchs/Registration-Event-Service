using RegistrationEventService.Domain.Entities;

namespace RegistrationEventService.Domain.Abstractions;

/// <summary>
/// Persistence contract for the <see cref="User"/> aggregate.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
}
