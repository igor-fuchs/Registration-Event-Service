namespace EventHandler.Models;

/// <summary>
/// Represents an SNS message wrapped around the actual event payload.
/// This is how Lambda receives messages from SNS.
/// </summary>
public sealed class SnsMessage
{
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, SnsMessageAttribute> MessageAttributes { get; set; } = new();
}

/// <summary>
/// SNS message attribute containing the event type.
/// </summary>
public sealed class SnsMessageAttribute
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
