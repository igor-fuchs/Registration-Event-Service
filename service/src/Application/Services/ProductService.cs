using RegistrationEventService.Application.Abstractions;
using RegistrationEventService.Application.DTOs;
using RegistrationEventService.Domain.Abstractions;
using RegistrationEventService.Domain.Entities;
using RegistrationEventService.Domain.Events;
using RegistrationEventService.Domain.Exceptions;

namespace RegistrationEventService.Application.Services;

/// <summary>
/// Implements product registration workflows.
/// Coordinates persistence and event publication while keeping domain rules intact.
/// </summary>
public sealed class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IEventPublisher eventPublisher
        )
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _eventPublisher = eventPublisher;
    }

    /// <inheritdoc />
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        // Check for duplicate SKU
        var existingProduct = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct is not null)
        {
            throw new ProductAlreadyExistsException(request.Sku);
        }

        // Create and persist the product
        var product = Product.Create(request.Name, request.Sku, request.Supplier, request.Price, request.Description);
        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Publish domain event (fire-and-forget to SNS)
        var @event = new ProductCreatedEvent(product.Id, product.Name, product.Sku, product.Supplier, product.Price, product.CreatedAt);
        await _eventPublisher.PublishAsync(@event, cancellationToken);

        return MapToResponse(product);
    }

    /// <inheritdoc />
    public async Task<ProductResponse> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _productRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new ProductNotFoundException(id);
        }

        return MapToResponse(product);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ProductResponse>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _productRepository.GetAllAsync(cancellationToken);
        return products.Select(MapToResponse).ToList();
    }

    private static ProductResponse MapToResponse(Product product) =>
        new(product.Id, product.Name, product.Sku, product.Supplier, product.Price, product.Description, product.CreatedAt);
}
