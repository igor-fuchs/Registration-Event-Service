namespace RegistrationEventService.Domain.Events;

/// <summary>
/// Domain event raised when a new user is successfully persisted.
/// Published to an external broker (e.g., AWS SNS) so that downstream
/// consumers can react asynchronously (logging, email, auditing, etc.).
/// </summary>
public sealed record UserCreatedEvent(
    int UserId,
    string Email,
    DateTime CreatedAt) : IDomainEvent;
