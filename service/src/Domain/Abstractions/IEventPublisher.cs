using RegistrationEventService.Domain.Events;

namespace RegistrationEventService.Domain.Abstractions;

/// <summary>
/// Abstraction for publishing domain events to an external broker.
/// The API publishes events without knowing who (or what) will consume them,
/// enforcing the decoupled, event-driven architecture.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IDomainEvent;
}
