namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

public sealed record WalletDepositResponse(
    string Message,
    WalletSummaryResponse Wallet,
    WalletMovementResponse Movement);
