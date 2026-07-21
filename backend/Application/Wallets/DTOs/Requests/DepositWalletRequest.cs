namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Requests;

public sealed record DepositWalletRequest(
    decimal Amount,
    string? Description);
