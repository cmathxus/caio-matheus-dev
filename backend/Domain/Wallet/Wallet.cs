namespace CaioMatheusDev.Api.Domain.Wallet;

public sealed record Wallet(
    Guid Id,
    Guid UserId,
    decimal Balance,
    string Currency,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
