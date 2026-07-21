namespace CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

public sealed record WalletMovementResponse(
    Guid Id,
    string Type,
    decimal Amount,
    string Description,
    string? RelatedUserName,
    string? RelatedUserEmail,
    Guid? TransferId,
    DateTimeOffset CreatedAt);
