using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Requests;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Authorize]
[Route("api/wallet")]
public sealed class WalletController(IWalletService walletService) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<WalletSummaryResponse>>> GetWallet(
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<WalletSummaryResponse>();
        }

        var result = await walletService.GetWalletAsync(userId.Value, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<WalletDepositResponse>>> Deposit(
        DepositWalletRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<WalletDepositResponse>();
        }

        var result = await walletService.DepositAsync(userId.Value, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpPost("transfers")]
    public async Task<ActionResult<ApiResponse<WalletTransferResponse>>> Transfer(
        CreateWalletTransferRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<WalletTransferResponse>();
        }

        var result = await walletService.TransferAsync(userId.Value, request, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    [HttpGet("statement")]
    public async Task<ActionResult<ApiResponse<WalletStatementResponse>>> GetStatement(
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<WalletStatementResponse>();
        }

        var result = await walletService.GetStatementAsync(userId.Value, cancellationToken);

        return FromResult(result, ResolveFailureStatus(result.Error));
    }

    private static int ResolveFailureStatus(ApiError? error) =>
        error?.Code switch
        {
            "user_not_found" => StatusCodes.Status401Unauthorized,
            "receiver_not_found" => StatusCodes.Status404NotFound,
            "insufficient_balance" => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };
}
