namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class WalletMovementEntity
{
    public Guid Id { get; set; }

    public Guid WalletId { get; set; }

    public Guid? TransferId { get; set; }

    public string Type { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public WalletEntity? Wallet { get; set; }

    public WalletTransferEntity? Transfer { get; set; }
}
