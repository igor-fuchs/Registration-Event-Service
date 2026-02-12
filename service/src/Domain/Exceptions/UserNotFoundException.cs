namespace RegistrationEventService.Domain.Exceptions;

/// <summary>
/// Thrown when a requested user cannot be found.
/// </summary>
public sealed class UserNotFoundException : DomainException
{
    public UserNotFoundException(int userId)
        : base($"User with ID '{userId}' was not found.") { }
}
