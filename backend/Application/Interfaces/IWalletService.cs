using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Requests;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IWalletService
{
    Task<Result<WalletSummaryResponse>> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<Result<WalletDepositResponse>> DepositAsync(
        Guid userId,
        DepositWalletRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<WalletTransferResponse>> TransferAsync(
        Guid userId,
        CreateWalletTransferRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<WalletStatementResponse>> GetStatementAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
