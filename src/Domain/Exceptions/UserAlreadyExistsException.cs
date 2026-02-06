namespace RegistrationEventService.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to register a user whose email is already taken.
/// </summary>
public sealed class UserAlreadyExistsException : DomainException
{
    public UserAlreadyExistsException(string email)
        : base($"A user with the email '{email}' already exists.") { }
}
