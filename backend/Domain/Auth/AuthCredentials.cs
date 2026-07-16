namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record AuthCredentials(
    string Email,
    string Password);

public sealed record RegisterCredentials(
    string Name,
    string Email,
    string Password);

