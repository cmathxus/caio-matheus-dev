namespace CaioMatheusDev.Api.Domain.Wallet;

public sealed record WalletTransfer(
    Guid Id,
    Guid FromWalletId,
    Guid ToWalletId,
    decimal Amount,
    string Description,
    WalletTransferStatus Status,
    DateTimeOffset CreatedAt);
