namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthenticatedUser(
    string Message,
    AuthUserProfile User,
    IReadOnlyCollection<string> Claims);

