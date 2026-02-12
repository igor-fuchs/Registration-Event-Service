using Microsoft.Extensions.Logging;

namespace EventHandler.Services;

/// <summary>
/// Service for handling SNS event processing orchestration.
/// Routes events to appropriate handlers based on event type.
/// </summary>
public sealed class EventProcessingService
{
    private readonly EmailService _emailService;
    private readonly AuditService _auditService;
    private readonly ILogger<EventProcessingService> _logger;

    public EventProcessingService(
        EmailService emailService,
        AuditService auditService,
        ILogger<EventProcessingService> logger)
    {
        _emailService = emailService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Routes incoming SNS messages to the appropriate handler.
    /// </summary>
    public async Task ProcessEventAsync(string eventType, string eventPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing event of type: {EventType}", eventType);

            switch (eventType)
            {
                case "UserCreatedEvent":
                    await ProcessUserCreatedEventAsync(eventPayload, cancellationToken);
                    break;

                case "ProductCreatedEvent":
                    await ProcessProductCreatedEventAsync(eventPayload, cancellationToken);
                    break;

                default:
                    _logger.LogWarning("Unknown event type received: {EventType}", eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event of type {EventType}", eventType);
            throw;
        }
    }

    /// <summary>
    /// Handles UserCreatedEvent by sending welcome email and logging audit trail.
    /// </summary>
    private async Task ProcessUserCreatedEventAsync(string eventPayload, CancellationToken cancellationToken)
    {
        try
        {
            var userEvent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eventPayload);

            if (userEvent == null)
            {
                _logger.LogError("Failed to deserialize UserCreatedEvent from payload");
                return;
            }

            var userId = int.Parse(userEvent["userId"]?.ToString() ?? "0");
            var email = userEvent["email"]?.ToString() ?? string.Empty;
            var createdAt = DateTime.Parse(userEvent["createdAt"]?.ToString() ?? DateTime.UtcNow.ToString("O"));

            // Execute downstream actions asynchronously
            await Task.WhenAll(
                _emailService.SendWelcomeEmailAsync(email, createdAt, cancellationToken),
                _auditService.LogUserCreatedAsync(userId, email, createdAt, cancellationToken));

            _logger.LogInformation("UserCreatedEvent processed successfully for UserId: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process UserCreatedEvent");
            throw;
        }
    }

    /// <summary>
    /// Handles ProductCreatedEvent by sending product notification and logging audit trail.
    /// </summary>
    private async Task ProcessProductCreatedEventAsync(string eventPayload, CancellationToken cancellationToken)
    {
        try
        {
            var productEvent = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(eventPayload);

            if (productEvent == null)
            {
                _logger.LogError("Failed to deserialize ProductCreatedEvent from payload");
                return;
            }

            var productId = int.Parse(productEvent["productId"]?.ToString() ?? "0");
            var name = productEvent["name"]?.ToString() ?? string.Empty;
            var sku = productEvent["sku"]?.ToString() ?? string.Empty;
            var price = decimal.Parse(productEvent["price"]?.ToString() ?? "0");
            var createdAt = DateTime.Parse(productEvent["createdAt"]?.ToString() ?? DateTime.UtcNow.ToString("O"));

            // Execute downstream actions asynchronously
            await Task.WhenAll(
                _emailService.SendProductNotificationEmailAsync(name, sku, price, cancellationToken),
                _auditService.LogProductCreatedAsync(productId, name, sku, price, createdAt, cancellationToken));

            _logger.LogInformation("ProductCreatedEvent processed successfully for ProductId: {ProductId}", productId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process ProductCreatedEvent");
            throw;
        }
    }
}
