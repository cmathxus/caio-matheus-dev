namespace CaioMatheusDev.Api.Domain.Auth;

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

public sealed record PasswordResetRequestResult(
    string Message,
    bool EmailConfigured,
    DateTimeOffset? ExpiresAt,
    string? DevelopmentToken,
    string? ResetUrl);

public sealed record PasswordResetConfirmation(string Message);

