namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record PasswordResetToken(
    Guid Id,
    Guid UserId,
    string TokenHash,
    DateTimeOffset CreatedAt,
    DateTimeOffset ExpiresAt,
    DateTimeOffset? UsedAt);

