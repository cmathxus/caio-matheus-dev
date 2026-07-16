namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthUser(
    Guid Id,
    string Name,
    string Email,
    string PasswordHash,
    string PasswordSalt,
    DateTimeOffset CreatedAt);

