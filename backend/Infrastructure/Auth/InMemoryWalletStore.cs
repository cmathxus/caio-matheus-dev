using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Wallets;
using CaioMatheusDev.Api.Domain.Wallet;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class InMemoryWalletStore : IWalletStore
{
    private const decimal InitialSandboxBalance = 1_000m;
    private readonly object gate = new();
    private readonly Dictionary<Guid, WalletState> walletsByUserId = [];
    private readonly List<WalletTransfer> transfers = [];
    private readonly List<WalletMovement> movements = [];

    public Task<Wallet> GetOrCreateWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            return Task.FromResult(GetOrCreateWallet(userId, DateTimeOffset.UtcNow).ToDomain());
        }
    }

    public Task<WalletDepositOperation> DepositAsync(
        Guid userId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            var wallet = GetOrCreateWallet(userId, now);
            wallet.Balance += amount;
            wallet.UpdatedAt = now;

            var movement = new WalletMovement(
                Guid.NewGuid(),
                wallet.Id,
                null,
                WalletMovementType.Credit,
                amount,
                description,
                now);

            movements.Add(movement);

            return Task.FromResult(new WalletDepositOperation(wallet.ToDomain(), movement));
        }
    }

    public Task<WalletTransferOperation?> TransferAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            var senderWallet = GetOrCreateWallet(senderUserId, now);
            var receiverWallet = GetOrCreateWallet(receiverUserId, now);

            if (senderWallet.Balance < amount)
            {
                return Task.FromResult<WalletTransferOperation?>(null);
            }

            senderWallet.Balance -= amount;
            senderWallet.UpdatedAt = now;
            receiverWallet.Balance += amount;
            receiverWallet.UpdatedAt = now;

            var transfer = new WalletTransfer(
                Guid.NewGuid(),
                senderWallet.Id,
                receiverWallet.Id,
                amount,
                description,
                WalletTransferStatus.Completed,
                now);

            var debitMovement = new WalletMovement(
                Guid.NewGuid(),
                senderWallet.Id,
                transfer.Id,
                WalletMovementType.Debit,
                amount,
                description,
                now);

            var creditMovement = new WalletMovement(
                Guid.NewGuid(),
                receiverWallet.Id,
                transfer.Id,
                WalletMovementType.Credit,
                amount,
                description,
                now);

            transfers.Add(transfer);
            movements.Add(debitMovement);
            movements.Add(creditMovement);

            return Task.FromResult<WalletTransferOperation?>(new WalletTransferOperation(
                transfer,
                senderWallet.ToDomain(),
                receiverWallet.ToDomain(),
                debitMovement,
                creditMovement));
        }
    }

    public Task<IReadOnlyCollection<WalletMovementDetails>> GetStatementAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        lock (gate)
        {
            var wallet = GetOrCreateWallet(userId, DateTimeOffset.UtcNow);
            var statement = movements
                .Where(movement => movement.WalletId == wallet.Id)
                .OrderByDescending(movement => movement.CreatedAt)
                .Take(take)
                .Select(movement => new WalletMovementDetails(movement, null, null))
                .ToList();

            return Task.FromResult<IReadOnlyCollection<WalletMovementDetails>>(statement);
        }
    }

    private WalletState GetOrCreateWallet(Guid userId, DateTimeOffset now)
    {
        if (walletsByUserId.TryGetValue(userId, out var wallet))
        {
            return wallet;
        }

        wallet = new WalletState(
            Guid.NewGuid(),
            userId,
            InitialSandboxBalance,
            "RYO",
            now,
            now);

        walletsByUserId[userId] = wallet;
        movements.Add(new WalletMovement(
            Guid.NewGuid(),
            wallet.Id,
            null,
            WalletMovementType.Credit,
            InitialSandboxBalance,
            "Saldo inicial sandbox",
            now));

        return wallet;
    }

    private sealed class WalletState(
        Guid id,
        Guid userId,
        decimal balance,
        string currency,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
    {
        public Guid Id { get; } = id;

        public Guid UserId { get; } = userId;

        public decimal Balance { get; set; } = balance;

        public string Currency { get; } = currency;

        public DateTimeOffset CreatedAt { get; } = createdAt;

        public DateTimeOffset UpdatedAt { get; set; } = updatedAt;

        public Wallet ToDomain() => new(Id, UserId, Balance, Currency, CreatedAt, UpdatedAt);
    }
}
