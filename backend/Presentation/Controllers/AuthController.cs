using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Route("api/auth")]
public sealed class AuthController(IAuthLabService authLabService) : ApiControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthSession>>> Register(
        RegisterCredentials request,
        CancellationToken cancellationToken)
    {
        var result = await authLabService.RegisterAsync(request, cancellationToken);
        return FromResult(result, result.Error?.Code is "email_already_registered"
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest);
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthSession>>> Login(
        AuthCredentials request,
        CancellationToken cancellationToken)
    {
        var result = await authLabService.LoginAsync(request, cancellationToken);
        return FromResult(result, result.Error?.Code is "invalid_credentials"
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status400BadRequest);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<AuthenticatedUser>>> Me(
        CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();

        if (userId is null)
        {
            return InvalidTokenPayload<AuthenticatedUser>();
        }

        var result = await authLabService.GetCurrentUserAsync(
            userId.Value,
            GetAuthenticatedClaimSummary(),
            cancellationToken);

        return FromResult(result, StatusCodes.Status401Unauthorized);
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse<PasswordResetRequestResult>>> ForgotPassword(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authLabService.ForgotPasswordAsync(request, cancellationToken);
        return FromResult(result, result.Error?.Code is "email_unavailable"
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status400BadRequest);
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<PasswordResetConfirmation>>> ResetPassword(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authLabService.ResetPasswordAsync(request, cancellationToken);
        return FromResult(result, result.Error?.Code is "invalid_reset_token"
            ? StatusCodes.Status401Unauthorized
            : StatusCodes.Status400BadRequest);
    }
}
