namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Requests;

public sealed record CreateWalletTransferRequest(
    string ToEmail,
    decimal Amount,
    string? Description);
