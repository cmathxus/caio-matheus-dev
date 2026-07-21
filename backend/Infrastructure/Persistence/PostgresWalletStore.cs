using System.Data;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Wallets;
using CaioMatheusDev.Api.Domain.Wallet;
using CaioMatheusDev.Api.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CaioMatheusDev.Api.Infrastructure.Persistence;

public sealed class PostgresWalletStore(PortfolioDbContext dbContext) : IWalletStore
{
    private const decimal InitialSandboxBalance = 1_000m;
    private const string DefaultCurrency = "RYO";

    public async Task<Wallet> GetOrCreateWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var entity = await GetOrCreateWalletEntityAsync(userId, now, cancellationToken);

        return entity.ToDomain();
    }

    public async Task<WalletDepositOperation> DepositAsync(
        Guid userId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var wallet = await GetOrCreateWalletEntityAsync(userId, now, cancellationToken);

        wallet.Balance += amount;
        wallet.UpdatedAt = now;

        var movement = new WalletMovementEntity
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = WalletMovementType.Credit.ToString(),
            Amount = amount,
            Description = description,
            CreatedAt = now
        };

        dbContext.WalletMovements.Add(movement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new WalletDepositOperation(wallet.ToDomain(), movement.ToDomain());
    }

    public async Task<WalletTransferOperation?> TransferAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var senderWallet = await GetOrCreateWalletEntityAsync(senderUserId, now, cancellationToken);
        var receiverWallet = await GetOrCreateWalletEntityAsync(receiverUserId, now, cancellationToken);

        if (senderWallet.Balance < amount)
        {
            await transaction.RollbackAsync(cancellationToken);
            return null;
        }

        senderWallet.Balance -= amount;
        senderWallet.UpdatedAt = now;
        receiverWallet.Balance += amount;
        receiverWallet.UpdatedAt = now;

        var transfer = new WalletTransferEntity
        {
            Id = Guid.NewGuid(),
            FromWalletId = senderWallet.Id,
            ToWalletId = receiverWallet.Id,
            Amount = amount,
            Description = description,
            Status = WalletTransferStatus.Completed.ToString(),
            CreatedAt = now
        };

        var debitMovement = new WalletMovementEntity
        {
            Id = Guid.NewGuid(),
            WalletId = senderWallet.Id,
            TransferId = transfer.Id,
            Type = WalletMovementType.Debit.ToString(),
            Amount = amount,
            Description = description,
            CreatedAt = now
        };

        var creditMovement = new WalletMovementEntity
        {
            Id = Guid.NewGuid(),
            WalletId = receiverWallet.Id,
            TransferId = transfer.Id,
            Type = WalletMovementType.Credit.ToString(),
            Amount = amount,
            Description = description,
            CreatedAt = now
        };

        dbContext.WalletTransfers.Add(transfer);
        dbContext.WalletMovements.AddRange(debitMovement, creditMovement);
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new WalletTransferOperation(
            transfer.ToDomain(),
            senderWallet.ToDomain(),
            receiverWallet.ToDomain(),
            debitMovement.ToDomain(),
            creditMovement.ToDomain());
    }

    public async Task<IReadOnlyCollection<WalletMovementDetails>> GetStatementAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        var wallet = await GetOrCreateWalletEntityAsync(userId, DateTimeOffset.UtcNow, cancellationToken);
        var movements = await dbContext.WalletMovements
            .AsNoTracking()
            .Include(movement => movement.Transfer)
                .ThenInclude(transfer => transfer!.FromWallet)
                .ThenInclude(walletEntity => walletEntity!.User)
            .Include(movement => movement.Transfer)
                .ThenInclude(transfer => transfer!.ToWallet)
                .ThenInclude(walletEntity => walletEntity!.User)
            .Where(movement => movement.WalletId == wallet.Id)
            .OrderByDescending(movement => movement.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

        return movements.Select(movement =>
        {
            var relatedUser = movement.Type == WalletMovementType.Debit.ToString()
                ? movement.Transfer?.ToWallet?.User
                : movement.Transfer?.FromWallet?.User;

            return new WalletMovementDetails(
                movement.ToDomain(),
                relatedUser?.Name,
                relatedUser?.Email);
        }).ToList();
    }

    private async Task<WalletEntity> GetOrCreateWalletEntityAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        var wallet = await dbContext.Wallets
            .FirstOrDefaultAsync(entity => entity.UserId == userId, cancellationToken);

        if (wallet is not null)
        {
            return wallet;
        }

        wallet = new WalletEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Balance = InitialSandboxBalance,
            Currency = DefaultCurrency,
            CreatedAt = now,
            UpdatedAt = now
        };

        var initialMovement = new WalletMovementEntity
        {
            Id = Guid.NewGuid(),
            WalletId = wallet.Id,
            Type = WalletMovementType.Credit.ToString(),
            Amount = InitialSandboxBalance,
            Description = "Saldo inicial sandbox",
            CreatedAt = now
        };

        dbContext.Wallets.Add(wallet);
        dbContext.WalletMovements.Add(initialMovement);
        await dbContext.SaveChangesAsync(cancellationToken);

        return wallet;
    }
}

file static class WalletEntityMapper
{
    public static Wallet ToDomain(this WalletEntity entity) =>
        new(
            entity.Id,
            entity.UserId,
            entity.Balance,
            entity.Currency,
            entity.CreatedAt,
            entity.UpdatedAt);

    public static WalletTransfer ToDomain(this WalletTransferEntity entity) =>
        new(
            entity.Id,
            entity.FromWalletId,
            entity.ToWalletId,
            entity.Amount,
            entity.Description,
            Enum.Parse<WalletTransferStatus>(entity.Status),
            entity.CreatedAt);

    public static WalletMovement ToDomain(this WalletMovementEntity entity) =>
        new(
            entity.Id,
            entity.WalletId,
            entity.TransferId,
            Enum.Parse<WalletMovementType>(entity.Type),
            entity.Amount,
            entity.Description,
            entity.CreatedAt);
}
