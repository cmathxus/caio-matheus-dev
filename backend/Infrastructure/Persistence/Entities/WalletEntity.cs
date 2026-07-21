namespace CaioMatheusDev.Api.Infrastructure.Persistence.Entities;

public sealed class WalletEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public decimal Balance { get; set; }

    public string Currency { get; set; } = "BRL";

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public AuthUserEntity? User { get; set; }

    public ICollection<WalletTransferEntity> OutgoingTransfers { get; set; } = [];

    public ICollection<WalletTransferEntity> IncomingTransfers { get; set; } = [];

    public ICollection<WalletMovementEntity> Movements { get; set; } = [];
}
