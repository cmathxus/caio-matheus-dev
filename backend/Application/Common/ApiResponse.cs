namespace CaioMatheusDev.Api.Application.Common;

public sealed record ApiResponse<T>(bool Success, T? Data, ApiError? Error, DateTimeOffset RespondedAt)
{
    public static ApiResponse<T> Ok(T data) => new(true, data, null, DateTimeOffset.UtcNow);

    public static ApiResponse<T> Fail(ApiError error) => new(false, default, error, DateTimeOffset.UtcNow);
}
