namespace RegistrationEventService.Domain.Abstractions;

/// <summary>
/// Represents an atomic unit of work.
/// Typically backed by a database transaction (e.g., EF Core DbContext).
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
