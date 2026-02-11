using System.ComponentModel.DataAnnotations;

namespace RegistrationEventService.Application.DTOs;

/// <summary>
/// Data transfer object for product creation requests.
/// Validated at the API boundary before reaching application logic.
/// </summary>
public sealed record CreateProductRequest(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 150 characters.")]
    string Name,

    [Required(ErrorMessage = "SKU is required.")]
    [StringLength(64, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 64 characters.")]
    string Sku,

    [Required(ErrorMessage = "Supplier is required.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Supplier must be between 2 and 150 characters.")]
    string Supplier,

    [Range(typeof(decimal), "0.01", "9999999", ErrorMessage = "Price must be greater than zero.")]
    decimal Price,

    [StringLength(1000, ErrorMessage = "Description must not exceed 1000 characters.")]
    string? Description);
