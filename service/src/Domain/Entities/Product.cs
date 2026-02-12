namespace RegistrationEventService.Domain.Entities;

/// <summary>
/// Represents a registered product in the system.
/// This is the core domain entity — the source of truth for product data.
/// </summary>
public sealed class Product
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Sku { get; private set; } = string.Empty;
    public string Supplier { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private Product() { }

    /// <summary>
    /// Factory method that creates a new <see cref="Product"/> with the current UTC timestamp.
    /// </summary>
    public static Product Create(string name, string sku, string supplier, decimal price, string? description)
    {
        return new Product
        {
            Name = name,
            Sku = sku,
            Supplier = supplier,
            Price = price,
            Description = description ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };
    }
}
