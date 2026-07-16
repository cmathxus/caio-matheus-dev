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
}
