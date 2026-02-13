using System.Text.Json;
using EventHandler.Events;
using Microsoft.Extensions.Logging;

namespace EventHandler.Services;

/// <summary>
/// Service for handling SNS event processing orchestration.
/// Routes events to appropriate handlers based on event type.
/// Deserializes events using strongly-typed contracts for type safety.
/// </summary>
public sealed class EventProcessingService
{
    private readonly EmailService _emailService;
    private readonly AuditService _auditService;
    private readonly ILogger<EventProcessingService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

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
    /// Routes incoming SNS messages to the appropriate handler using strongly-typed event contracts.
    /// </summary>
    public async Task ProcessEventAsync(string eventType, string eventPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing event of type: {EventType}", eventType);

            switch (eventType)
            {
                case nameof(UserCreatedEvent):
                    await ProcessUserCreatedEventAsync(eventPayload, cancellationToken);
                    break;

                case nameof(ProductCreatedEvent):
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
    /// Uses strongly-typed event contract for type safety.
    /// </summary>
    private async Task ProcessUserCreatedEventAsync(string eventPayload, CancellationToken cancellationToken)
    {
        try
        {
            var userEvent = JsonSerializer.Deserialize<UserCreatedEvent>(eventPayload, JsonOptions);

            if (userEvent == null)
            {
                _logger.LogError("Failed to deserialize UserCreatedEvent from payload: {Payload}", eventPayload);
                return;
            }

            // Execute downstream actions asynchronously with strongly-typed event
            await Task.WhenAll(
                _emailService.SendWelcomeEmailAsync(userEvent.Email, userEvent.CreatedAt, cancellationToken),
                _auditService.LogUserCreatedAsync(userEvent.UserId, userEvent.Email, userEvent.CreatedAt, cancellationToken));

            _logger.LogInformation("UserCreatedEvent processed successfully for UserId: {UserId}, Email: {Email}", 
                userEvent.UserId, userEvent.Email);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for UserCreatedEvent");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process UserCreatedEvent");
            throw;
        }
    }

    /// <summary>
    /// Handles ProductCreatedEvent by sending product notification and logging audit trail.
    /// Uses strongly-typed event contract for type safety.
    /// </summary>
    private async Task ProcessProductCreatedEventAsync(string eventPayload, CancellationToken cancellationToken)
    {
        try
        {
            var productEvent = JsonSerializer.Deserialize<ProductCreatedEvent>(eventPayload, JsonOptions);

            if (productEvent == null)
            {
                _logger.LogError("Failed to deserialize ProductCreatedEvent from payload: {Payload}", eventPayload);
                return;
            }

            // Execute downstream actions asynchronously with strongly-typed event
            await Task.WhenAll(
                _emailService.SendProductNotificationEmailAsync(productEvent.Name, productEvent.Sku, productEvent.Price, cancellationToken),
                _auditService.LogProductCreatedAsync(productEvent.ProductId, productEvent.Name, productEvent.Sku, 
                    productEvent.Price, productEvent.CreatedAt, cancellationToken));

            _logger.LogInformation("ProductCreatedEvent processed successfully for ProductId: {ProductId}, Name: {Name}", 
                productEvent.ProductId, productEvent.Name);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization failed for ProductCreatedEvent");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process ProductCreatedEvent");
            throw;
        }
    }
}
