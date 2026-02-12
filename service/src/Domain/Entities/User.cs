namespace RegistrationEventService.Domain.Entities;

/// <summary>
/// Represents a registered user in the system.
/// This is the core domain entity — the source of truth for user data.
/// </summary>
public sealed class User
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Required by EF Core for materialization.
    /// </summary>
    private User() { }

    /// <summary>
    /// Factory method that creates a new <see cref="User"/> with the current UTC timestamp.
    /// </summary>
    public static User Create(string name, string email)
    {
        return new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.UtcNow
        };
    }
}
