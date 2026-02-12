using Microsoft.Extensions.Logging;

namespace EventHandler.Services;

/// <summary>
/// Service responsible for audit logging.
/// Tracks all events processed by the Lambda for compliance and debugging.
/// </summary>
public sealed class AuditService
{
    private readonly ILogger<AuditService> _logger;

    public AuditService(ILogger<AuditService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs a user creation event for audit purposes.
    /// </summary>
    public async Task LogUserCreatedAsync(int userId, string email, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        try
        {
            // In production, this could persist to a database, CloudWatch, or security system
            await Task.Run(() =>
            {
                _logger.LogInformation(
                    "[AUDIT] User created - UserId: {UserId}, Email: {Email}, CreatedAt: {CreatedAt}",
                    userId,
                    email,
                    createdAt);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AUDIT] Failed to log user creation for UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Logs a product creation event for audit purposes.
    /// </summary>
    public async Task LogProductCreatedAsync(
        int productId,
        string name,
        string sku,
        decimal price,
        DateTime createdAt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // In production, this could persist to a database, CloudWatch, or security system
            await Task.Run(() =>
            {
                _logger.LogInformation(
                    "[AUDIT] Product created - ProductId: {ProductId}, Name: {Name}, SKU: {Sku}, Price: {Price}, CreatedAt: {CreatedAt}",
                    productId,
                    name,
                    sku,
                    price,
                    createdAt);
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[AUDIT] Failed to log product creation for ProductId: {ProductId}", productId);
            throw;
        }
    }
}
