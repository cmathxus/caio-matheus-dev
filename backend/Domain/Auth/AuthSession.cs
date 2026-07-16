namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthSession(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt,
    AuthUserProfile User);

