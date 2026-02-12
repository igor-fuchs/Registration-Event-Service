namespace RegistrationEventService.Infrastructure.Messaging;

/// <summary>
/// Configuration options for AWS SNS event publishing.
/// </summary>
public sealed class SnsOptions
{
    public const string SectionName = "Aws:Sns";

    /// <summary>
    /// The ARN of the SNS topic to publish events to.
    /// Example: arn:aws:sns:us-east-1:123456789012:UserCreated
    /// </summary>
    public string TopicArn { get; set; } = string.Empty;

    /// <summary>
    /// AWS region where the SNS topic resides.
    /// </summary>
    public string Region { get; set; } = "us-east-1";
}
