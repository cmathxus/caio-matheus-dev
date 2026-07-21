using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CaioMatheusDev.Api.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> FromResult<T>(
        Result<T> result,
        int failureStatus = StatusCodes.Status400BadRequest)
    {
        if (result.IsSuccess)
        {
            return Ok(ApiResponse<T>.Ok(result.Value!));
        }

        return StatusCode(failureStatus, ApiResponse<T>.Fail(result.Error!));
    }

    protected Guid? GetAuthenticatedUserId()
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                      User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(subject, out var userId)
            ? userId
            : null;
    }

    protected IReadOnlyCollection<string> GetAuthenticatedClaimSummary() =>
        User.Claims
            .Select(claim => $"{claim.Type}: {claim.Value}")
            .ToList();

    protected ActionResult<ApiResponse<T>> InvalidTokenPayload<T>() =>
        FromResult(
            Result<T>.Fail("invalid_token_payload", "JWT payload does not contain a valid user id."),
            StatusCodes.Status401Unauthorized);
}
