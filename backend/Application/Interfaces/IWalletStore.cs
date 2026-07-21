using CaioMatheusDev.Api.Application.Wallets;
using CaioMatheusDev.Api.Domain.Wallet;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IWalletStore
{
    Task<Wallet> GetOrCreateWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<WalletDepositOperation> DepositAsync(
        Guid userId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<WalletTransferOperation?> TransferAsync(
        Guid senderUserId,
        Guid receiverUserId,
        decimal amount,
        string description,
        DateTimeOffset now,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<WalletMovementDetails>> GetStatementAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default);
}
