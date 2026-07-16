namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthUserProfile(
    Guid Id,
    string Name,
    string Email,
    DateTimeOffset CreatedAt);

