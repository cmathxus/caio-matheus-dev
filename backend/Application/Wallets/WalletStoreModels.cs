using CaioMatheusDev.Api.Domain.Wallet;

namespace CaioMatheusDev.Api.Application.Wallets;

public sealed record WalletDepositOperation(
    Wallet Wallet,
    WalletMovement Movement);

public sealed record WalletTransferOperation(
    WalletTransfer Transfer,
    Wallet SenderWallet,
    Wallet ReceiverWallet,
    WalletMovement DebitMovement,
    WalletMovement CreditMovement);

public sealed record WalletMovementDetails(
    WalletMovement Movement,
    string? RelatedUserName,
    string? RelatedUserEmail);
