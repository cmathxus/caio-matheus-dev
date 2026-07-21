namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

public sealed record WalletTransferResponse(
    Guid TransferId,
    string Status,
    decimal Amount,
    string Description,
    string FromEmail,
    string ToEmail,
    WalletSummaryResponse Wallet,
    DateTimeOffset CreatedAt);
