namespace EventHandler.Events;

/// <summary>
/// Event model that is serialized from SNS message.
/// Represents a user creation event from the main API.
/// </summary>
public sealed record UserCreatedEvent(
    int UserId,
    string Email,
    DateTime CreatedAt);
