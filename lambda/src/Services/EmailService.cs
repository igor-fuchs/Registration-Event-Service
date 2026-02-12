using Microsoft.Extensions.Logging;

namespace EventHandler.Services;

/// <summary>
/// Service that simulates email sending.
/// In production, this would integrate with SendGrid, SES, or similar.
/// </summary>
public sealed class EmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Simulates sending a welcome email to a newly registered user.
    /// </summary>
    public async Task SendWelcomeEmailAsync(string email, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email sending delay
            await Task.Delay(500, cancellationToken);

            _logger.LogInformation(
                "Welcome email sent to {Email} for user registered at {CreatedAt}",
                email,
                createdAt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Simulates sending a product notification email.
    /// </summary>
    public async Task SendProductNotificationEmailAsync(
        string productName,
        string sku,
        decimal price,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Simulate email sending delay
            await Task.Delay(500, cancellationToken);

            _logger.LogInformation(
                "Product notification email sent for {ProductName} (SKU: {Sku}, Price: {Price})",
                productName,
                sku,
                price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send product notification email for {ProductName}", productName);
            throw;
        }
    }
}
