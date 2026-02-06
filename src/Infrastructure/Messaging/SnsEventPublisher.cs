using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RegistrationEventService.Domain.Abstractions;

namespace RegistrationEventService.Infrastructure.Messaging;

/// <summary>
/// Publishes domain events to AWS SNS.
/// The API doesn't know or care who consumes these events — 
/// this supports true event-driven decoupling.
/// </summary>
public sealed class SnsEventPublisher : IEventPublisher
{
    private readonly IAmazonSimpleNotificationService _snsClient;
    private readonly SnsOptions _options;
    private readonly ILogger<SnsEventPublisher> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public SnsEventPublisher(
        IAmazonSimpleNotificationService snsClient,
        IOptions<SnsOptions> options,
        ILogger<SnsEventPublisher> logger)
    {
        _snsClient = snsClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class
    {
        var eventType = typeof(TEvent).Name;
        var message = JsonSerializer.Serialize(@event, JsonOptions);

        var request = new PublishRequest
        {
            TopicArn = _options.TopicArn,
            Message = message,
            MessageAttributes = new Dictionary<string, MessageAttributeValue>
            {
                ["eventType"] = new()
                {
                    DataType = "String",
                    StringValue = eventType
                }
            }
        };

        try
        {
            var response = await _snsClient.PublishAsync(request, cancellationToken);
            _logger.LogInformation(
                "Published {EventType} to SNS. MessageId: {MessageId}",
                eventType,
                response.MessageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish {EventType} to SNS", eventType);
            throw;
        }
    }
}
