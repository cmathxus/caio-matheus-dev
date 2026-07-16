namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class PasswordResetTokenEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? UsedAt { get; set; }

    public AuthUserEntity? User { get; set; }
}

