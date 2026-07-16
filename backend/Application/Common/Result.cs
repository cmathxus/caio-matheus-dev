namespace CaioMatheusDev.Api.Application.Common;

public sealed record Result<T>(bool IsSuccess, T? Value, ApiError? Error)
{
    public static Result<T> Ok(T value) => new(true, value, null);

    public static Result<T> Fail(string code, string message) => new(false, default, new ApiError(code, message));
}

public sealed record ApiError(string Code, string Message);
