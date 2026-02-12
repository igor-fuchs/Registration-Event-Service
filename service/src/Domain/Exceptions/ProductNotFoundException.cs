namespace RegistrationEventService.Domain.Exceptions;

/// <summary>
/// Thrown when a requested product cannot be found.
/// </summary>
public sealed class ProductNotFoundException : DomainException
{
    public ProductNotFoundException(int productId)
        : base($"Product with ID '{productId}' was not found.") { }
}
