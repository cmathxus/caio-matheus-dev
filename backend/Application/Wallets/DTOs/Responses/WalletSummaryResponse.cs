namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

public sealed record WalletSummaryResponse(
    Guid WalletId,
    decimal Balance,
    string Currency,
    DateTimeOffset UpdatedAt);
