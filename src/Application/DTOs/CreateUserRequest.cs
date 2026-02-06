using System.ComponentModel.DataAnnotations;

namespace RegistrationEventService.Application.DTOs;

/// <summary>
/// Data transfer object for user creation requests.
/// Validated at the API boundary before reaching application logic.
/// </summary>
public sealed record CreateUserRequest(
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters.")]
    string Name,

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
    string Email);
