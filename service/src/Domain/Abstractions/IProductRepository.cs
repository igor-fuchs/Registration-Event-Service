using RegistrationEventService.Domain.Entities;

namespace RegistrationEventService.Domain.Abstractions;

/// <summary>
/// Persistence contract for the <see cref="Product"/> aggregate.
/// Implementations live in the Infrastructure layer.
/// </summary>
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Product?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Product product, CancellationToken cancellationToken = default);
}
