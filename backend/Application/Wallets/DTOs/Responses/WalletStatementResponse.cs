namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

public sealed record WalletStatementResponse(
    WalletSummaryResponse Wallet,
    IReadOnlyCollection<WalletMovementResponse> Movements,
    DateTimeOffset LoadedAt);
