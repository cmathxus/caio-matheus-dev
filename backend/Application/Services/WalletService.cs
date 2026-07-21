using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Wallets;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Requests;
using CaioMatheusDev.Api.Application.Wallets.DTOs.Responses;
using CaioMatheusDev.Api.Domain.Auth;
using CaioMatheusDev.Api.Domain.Wallet;

namespace CaioMatheusDev.Api.Application.Services;

public sealed class WalletService(
    IAuthUserStore authUserStore,
    IWalletStore walletStore) : IWalletService
{
    private const int StatementSize = 30;
    private const int DescriptionMaxLength = 160;
    private const decimal MaxSandboxAmount = 50_000m;

    public async Task<Result<WalletSummaryResponse>> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetExistingUserAsync(userId, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<WalletSummaryResponse>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var wallet = await walletStore.GetOrCreateWalletAsync(userResult.Value!.Id, cancellationToken);

        return Result<WalletSummaryResponse>.Ok(ToSummary(wallet));
    }

    public async Task<Result<WalletDepositResponse>> DepositAsync(
        Guid userId,
        DepositWalletRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetExistingUserAsync(userId, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<WalletDepositResponse>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var amountResult = NormalizeAmount(request.Amount);

        if (!amountResult.IsSuccess)
        {
            return Result<WalletDepositResponse>.Fail(amountResult.Error!.Code, amountResult.Error.Message);
        }

        var descriptionResult = NormalizeDescription(request.Description, "Depósito sandbox");

        if (!descriptionResult.IsSuccess)
        {
            return Result<WalletDepositResponse>.Fail(descriptionResult.Error!.Code, descriptionResult.Error.Message);
        }

        var operation = await walletStore.DepositAsync(
            userResult.Value!.Id,
            amountResult.Value,
            descriptionResult.Value!,
            DateTimeOffset.UtcNow,
            cancellationToken);

        return Result<WalletDepositResponse>.Ok(new WalletDepositResponse(
            "Saldo adicionado à carteira.",
            ToSummary(operation.Wallet),
            ToMovementResponse(operation.Movement, null, null)));
    }

    public async Task<Result<WalletTransferResponse>> TransferAsync(
        Guid userId,
        CreateWalletTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetExistingUserAsync(userId, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<WalletTransferResponse>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var sender = userResult.Value!;
        var receiverEmail = NormalizeEmail(request.ToEmail);

        if (!IsValidEmail(receiverEmail))
        {
            return Result<WalletTransferResponse>.Fail("invalid_receiver_email", "Informe um e-mail de destino válido.");
        }

        if (receiverEmail == sender.Email)
        {
            return Result<WalletTransferResponse>.Fail("self_transfer", "Não dá para transferir para a própria conta.");
        }

        var receiver = await authUserStore.FindByEmailAsync(receiverEmail, cancellationToken);

        if (receiver is null)
        {
            return Result<WalletTransferResponse>.Fail("receiver_not_found", "Chave de transferência não encontrada. Confira o e-mail do destinatário.");
        }

        var amountResult = NormalizeAmount(request.Amount);

        if (!amountResult.IsSuccess)
        {
            return Result<WalletTransferResponse>.Fail(amountResult.Error!.Code, amountResult.Error.Message);
        }

        var descriptionResult = NormalizeDescription(
            request.Description,
            $"Pix para {receiver.Email}");

        if (!descriptionResult.IsSuccess)
        {
            return Result<WalletTransferResponse>.Fail(descriptionResult.Error!.Code, descriptionResult.Error.Message);
        }

        var operation = await walletStore.TransferAsync(
            sender.Id,
            receiver.Id,
            amountResult.Value,
            descriptionResult.Value!,
            DateTimeOffset.UtcNow,
            cancellationToken);

        if (operation is null)
        {
            return Result<WalletTransferResponse>.Fail("insufficient_balance", "Saldo insuficiente para concluir a transferência.");
        }

        return Result<WalletTransferResponse>.Ok(new WalletTransferResponse(
            operation.Transfer.Id,
            operation.Transfer.Status.ToString(),
            operation.Transfer.Amount,
            operation.Transfer.Description,
            sender.Email,
            receiver.Email,
            ToSummary(operation.SenderWallet),
            operation.Transfer.CreatedAt));
    }

    public async Task<Result<WalletStatementResponse>> GetStatementAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userResult = await GetExistingUserAsync(userId, cancellationToken);

        if (!userResult.IsSuccess)
        {
            return Result<WalletStatementResponse>.Fail(userResult.Error!.Code, userResult.Error.Message);
        }

        var user = userResult.Value!;
        var wallet = await walletStore.GetOrCreateWalletAsync(user.Id, cancellationToken);
        var movements = await walletStore.GetStatementAsync(user.Id, StatementSize, cancellationToken);

        return Result<WalletStatementResponse>.Ok(new WalletStatementResponse(
            ToSummary(wallet),
            movements.Select(ToMovementResponse).ToList(),
            DateTimeOffset.UtcNow));
    }

    private async Task<Result<AuthUser>> GetExistingUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var user = await authUserStore.FindByIdAsync(userId, cancellationToken);

        return user is null
            ? Result<AuthUser>.Fail("user_not_found", "The token is valid, but the user no longer exists.")
            : Result<AuthUser>.Ok(user);
    }

    private static Result<decimal> NormalizeAmount(decimal amount)
    {
        if (amount <= 0)
        {
            return Result<decimal>.Fail("invalid_amount", "O valor precisa ser maior que zero.");
        }

        if (amount > MaxSandboxAmount)
        {
            return Result<decimal>.Fail("amount_too_high", $"O limite por operação sandbox é {MaxSandboxAmount:N2}.");
        }

        var normalized = decimal.Round(amount, 2);

        if (normalized != amount)
        {
            return Result<decimal>.Fail("invalid_amount_precision", "Use no máximo duas casas decimais.");
        }

        return Result<decimal>.Ok(normalized);
    }

    private static Result<string> NormalizeDescription(string? description, string fallback)
    {
        var normalized = string.IsNullOrWhiteSpace(description)
            ? fallback
            : description.Trim();

        if (normalized.Length > DescriptionMaxLength)
        {
            return Result<string>.Fail("description_too_long", $"A descrição deve ter no máximo {DescriptionMaxLength} caracteres.");
        }

        return Result<string>.Ok(normalized);
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email) =>
        email.Length <= 160 &&
        email.Contains('@', StringComparison.Ordinal) &&
        email.Contains('.', StringComparison.Ordinal);

    private static WalletSummaryResponse ToSummary(Wallet wallet) =>
        new(wallet.Id, wallet.Balance, wallet.Currency, wallet.UpdatedAt);

    private static WalletMovementResponse ToMovementResponse(WalletMovementDetails details) =>
        ToMovementResponse(details.Movement, details.RelatedUserName, details.RelatedUserEmail);

    private static WalletMovementResponse ToMovementResponse(
        WalletMovement movement,
        string? relatedUserName,
        string? relatedUserEmail) =>
        new(
            movement.Id,
            movement.Type.ToString(),
            movement.Amount,
            movement.Description,
            relatedUserName,
            relatedUserEmail,
            movement.TransferId,
            movement.CreatedAt);
}
