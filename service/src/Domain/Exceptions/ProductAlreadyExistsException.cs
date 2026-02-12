namespace RegistrationEventService.Domain.Exceptions;

/// <summary>
/// Thrown when attempting to register a product whose SKU is already taken.
/// </summary>
public sealed class ProductAlreadyExistsException : DomainException
{
    public ProductAlreadyExistsException(string sku)
        : base($"A product with the SKU '{sku}' already exists.") { }
}
