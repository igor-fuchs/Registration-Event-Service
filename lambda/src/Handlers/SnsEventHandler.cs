using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.Serialization.SystemTextJson;
using EventHandler.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

[assembly: LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]

namespace EventHandler.Handlers;

/// <summary>
/// AWS Lambda handler that processes SNS events from the Registration Event Service.
/// This is the entry point for Lambda invocations triggered by SNS notifications.
/// </summary>
public sealed class SnsEventHandler
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SnsEventHandler> _logger;
    private readonly EventProcessingService _eventProcessingService;

    /// <summary>
    /// Static constructor to initialize dependency injection once during Lambda warm start.
    /// </summary>
    static SnsEventHandler()
    {
        // Configure Serilog for Lambda logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .CreateLogger();
    }

    /// <summary>
    /// Default constructor required by Lambda runtime.
    /// Initializes dependency injection container.
    /// </summary>
    public SnsEventHandler()
    {
        _serviceProvider = new ServiceCollection()
            .AddLogging(builder =>
            {
                builder
                    .ClearProviders()
                    .AddSerilog();
            })
            .AddScoped<EmailService>()
            .AddScoped<AuditService>()
            .AddScoped<EventProcessingService>()
            .BuildServiceProvider();

        _logger = _serviceProvider.GetRequiredService<ILogger<SnsEventHandler>>();
        _eventProcessingService = _serviceProvider.GetRequiredService<EventProcessingService>();
    }

    /// <summary>
    /// Handles SNS events by processing each record's message.
    /// This method is invoked by the Lambda runtime when an SNS event is received.
    /// Handler format: EventHandler::EventHandler.Handlers.SnsEventHandler::FunctionHandler
    /// </summary>
    [LambdaSerializer(typeof(DefaultLambdaJsonSerializer))]
    public async Task<string> FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
    {
        _logger.LogInformation("Received SNS event with {RecordCount} records", snsEvent.Records.Count);

        var tasks = snsEvent.Records.Select(record =>
            ProcessSnsMessageAsync(record.Sns, context.RemainingTime));

        try
        {
            await Task.WhenAll(tasks);
            _logger.LogInformation("Successfully processed all SNS records");
            return $"Successfully processed {snsEvent.Records.Count} records";
        }
        catch (AggregateException ae)
        {
            _logger.LogError(ae, "One or more SNS records failed to process");
            throw;
        }
    }

    /// <summary>
    /// Processes a single SNS message by extracting event type and payload.
    /// </summary>
    private async Task ProcessSnsMessageAsync(SNSEvent.SNSMessage snsMessage, TimeSpan remainingTime)
    {
        try
        {
            _logger.LogDebug("Processing SNS message: {MessageId}", snsMessage.MessageId);

            // Extract event type from message attributes
            var eventType = "Unknown";
            if (snsMessage.MessageAttributes.TryGetValue("eventType", out var attr))
            {
                eventType = attr.Value;
            }

            // The actual event payload is in the Message property
            var eventPayload = snsMessage.Message;

            // Process the event with timeout protection
            var processingTask = _eventProcessingService.ProcessEventAsync(
                eventType,
                eventPayload,
                CancellationToken.None);

            // Wait with timeout based on remaining Lambda execution time
            var timeout = remainingTime > TimeSpan.FromSeconds(5)
                ? remainingTime - TimeSpan.FromSeconds(5)
                : TimeSpan.FromSeconds(10);

            await processingTask.ConfigureAwait(false);

            _logger.LogInformation(
                "Successfully processed SNS message {MessageId} of type {EventType}",
                snsMessage.MessageId,
                eventType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process SNS message {MessageId}", snsMessage.MessageId);
            throw;
        }
    }
}
