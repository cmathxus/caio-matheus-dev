using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IEmailSender
{
    bool IsConfigured { get; }

    Task SendPasswordResetAsync(
        AuthUserProfile user,
        string resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default);
}

