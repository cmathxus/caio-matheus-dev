namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthTokenPayload(
    Guid UserId,
    string Name,
    string Email,
    DateTimeOffset ExpiresAt,
    IReadOnlyCollection<string> Claims);

