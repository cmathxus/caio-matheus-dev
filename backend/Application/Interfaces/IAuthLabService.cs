using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IAuthLabService
{
    Task<Result<AuthSession>> RegisterAsync(RegisterCredentials credentials, CancellationToken cancellationToken = default);

    Task<Result<AuthSession>> LoginAsync(AuthCredentials credentials, CancellationToken cancellationToken = default);

    Task<Result<AuthenticatedUser>> GetCurrentUserAsync(string authorizationHeader, CancellationToken cancellationToken = default);

    Task<Result<PasswordResetRequestResult>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<PasswordResetConfirmation>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default);
}
