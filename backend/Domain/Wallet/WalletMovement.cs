namespace CaioMatheusDev.Api.Domain.Wallet;

public sealed record WalletMovement(
    Guid Id,
    Guid WalletId,
    Guid? TransferId,
    WalletMovementType Type,
    decimal Amount,
    string Description,
    DateTimeOffset CreatedAt);
