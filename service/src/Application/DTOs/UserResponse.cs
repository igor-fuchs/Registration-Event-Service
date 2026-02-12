namespace RegistrationEventService.Application.DTOs;

/// <summary>
/// Data transfer object representing user data returned by the API.
/// Keeps domain entity details hidden from external consumers.
/// </summary>
public sealed record UserResponse(
    int Id,
    string Name,
    string Email,
    DateTime CreatedAt);
