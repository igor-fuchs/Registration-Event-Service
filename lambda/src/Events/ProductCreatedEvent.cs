namespace EventHandler.Events;

/// <summary>
/// Event model that is serialized from SNS message.
/// Represents a product creation event from the main API.
/// </summary>
public sealed record ProductCreatedEvent(
    int ProductId,
    string Name,
    string Sku,
    string Supplier,
    decimal Price,
    DateTime CreatedAt);
