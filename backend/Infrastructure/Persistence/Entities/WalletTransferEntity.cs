namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class WalletTransferEntity
{
    public Guid Id { get; set; }

    public Guid FromWalletId { get; set; }

    public Guid ToWalletId { get; set; }

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public WalletEntity? FromWallet { get; set; }

    public WalletEntity? ToWallet { get; set; }

    public ICollection<WalletMovementEntity> Movements { get; set; } = [];
}
