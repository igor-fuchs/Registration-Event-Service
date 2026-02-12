using RegistrationEventService.Application.DTOs;

namespace RegistrationEventService.Application.Abstractions;

/// <summary>
/// Application service contract for product-related operations.
/// Orchestrates domain logic, persistence, and event publishing.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Creates a new product, persists it, and publishes a ProductCreated event.
    /// </summary>
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    Task<ProductResponse> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all registered products.
    /// </summary>
    Task<IReadOnlyList<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken = default);
}
