namespace RegistrationEventService.Application.DTOs;

/// <summary>
/// Data transfer object representing product data returned by the API.
/// Keeps domain entity details hidden from external consumers.
/// </summary>
public sealed record ProductResponse(
    int Id,
    string Name,
    string Sku,
    string Supplier,
    decimal Price,
    string Description,
    DateTime CreatedAt);
