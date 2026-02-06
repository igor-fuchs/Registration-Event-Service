namespace RegistrationEventService.Domain.Exceptions;

/// <summary>
/// Base class for all domain-specific exceptions.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}
