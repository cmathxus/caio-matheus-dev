using System.Security.Cryptography;
using System.Text;
using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Application.Options;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.Extensions.Options;

namespace CaioMatheusDev.Api.Application.Services;

public sealed class AuthLabService(
    IAuthUserStore userStore,
    IPasswordResetTokenStore passwordResetTokenStore,
    IJwtTokenService jwtTokenService,
    IEmailSender emailSender,
    IOptions<AuthFlowOptions> authFlowOptions) : IAuthLabService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 120_000;
    private readonly AuthFlowOptions flowOptions = authFlowOptions.Value;

    public async Task<Result<AuthSession>> RegisterAsync(
        RegisterCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var normalizedName = credentials.Name.Trim();
        var normalizedEmail = NormalizeEmail(credentials.Email);

        if (normalizedName.Length < 2)
        {
            return Result<AuthSession>.Fail("invalid_name", "Name must have at least 2 characters.");
        }

        if (!IsValidEmail(normalizedEmail))
        {
            return Result<AuthSession>.Fail("invalid_email", "A valid email is required.");
        }

        if (credentials.Password.Length < 8)
        {
            return Result<AuthSession>.Fail("weak_password", "Password must have at least 8 characters.");
        }

        if (await userStore.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthSession>.Fail("email_already_registered", "This email is already registered in the Auth Lab.");
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordHash = HashPassword(credentials.Password, salt);
        var user = new AuthUser(
            Guid.NewGuid(),
            normalizedName,
            normalizedEmail,
            Convert.ToBase64String(passwordHash),
            Convert.ToBase64String(salt),
            DateTimeOffset.UtcNow);

        await userStore.AddAsync(user, cancellationToken);

        return Result<AuthSession>.Ok(jwtTokenService.CreateSession(user));
    }

    public async Task<Result<AuthSession>> LoginAsync(
        AuthCredentials credentials,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(credentials.Email);
        var user = await userStore.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null || !VerifyPassword(credentials.Password, user))
        {
            return Result<AuthSession>.Fail("invalid_credentials", "Invalid email or password.");
        }

        return Result<AuthSession>.Ok(jwtTokenService.CreateSession(user));
    }

    public async Task<Result<AuthenticatedUser>> GetCurrentUserAsync(
        Guid userId,
        IReadOnlyCollection<string> claims,
        CancellationToken cancellationToken = default)
    {
        var user = await userStore.FindByIdAsync(userId, cancellationToken);

        if (user is null)
        {
            return Result<AuthenticatedUser>.Fail("user_not_found", "The token is valid, but the user no longer exists.");
        }

        var profile = new AuthUserProfile(user.Id, user.Name, user.Email, user.CreatedAt);
        var response = new AuthenticatedUser(
            "JWT accepted. This endpoint only responds with a valid Bearer token.",
            profile,
            claims);

        return Result<AuthenticatedUser>.Ok(response);
    }

    public async Task<Result<PasswordResetRequestResult>> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (!IsValidEmail(normalizedEmail))
        {
            return Result<PasswordResetRequestResult>.Fail("invalid_email", "A valid email is required.");
        }

        var genericMessage = "If this email exists in the Auth Lab, a reset link will be prepared.";
        var user = await userStore.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result<PasswordResetRequestResult>.Ok(new PasswordResetRequestResult(
                genericMessage,
                emailSender.IsConfigured,
                null,
                null,
                null));
        }

        var rawToken = Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.AddMinutes(Math.Max(5, flowOptions.PasswordResetMinutes));
        var token = new PasswordResetToken(
            Guid.NewGuid(),
            user.Id,
            HashResetToken(rawToken),
            now,
            expiresAt,
            null);
        var resetUrl = BuildResetUrl(normalizedEmail, rawToken);

        await passwordResetTokenStore.AddAsync(token, cancellationToken);

        try
        {
            await emailSender.SendPasswordResetAsync(
                new AuthUserProfile(user.Id, user.Name, user.Email, user.CreatedAt),
                resetUrl,
                expiresAt,
                cancellationToken);
        }
        catch
        {
            return Result<PasswordResetRequestResult>.Fail(
                "email_unavailable",
                "The reset token was created, but the email provider is unavailable.");
        }

        return Result<PasswordResetRequestResult>.Ok(new PasswordResetRequestResult(
            genericMessage,
            emailSender.IsConfigured,
            expiresAt,
            emailSender.IsConfigured ? null : rawToken,
            emailSender.IsConfigured ? null : resetUrl));
    }

    public async Task<Result<PasswordResetConfirmation>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = NormalizeEmail(request.Email);

        if (!IsValidEmail(normalizedEmail))
        {
            return Result<PasswordResetConfirmation>.Fail("invalid_email", "A valid email is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result<PasswordResetConfirmation>.Fail("missing_token", "Reset token is required.");
        }

        if (request.NewPassword.Length < 8)
        {
            return Result<PasswordResetConfirmation>.Fail("weak_password", "Password must have at least 8 characters.");
        }

        var user = await userStore.FindByEmailAsync(normalizedEmail, cancellationToken);

        if (user is null)
        {
            return Result<PasswordResetConfirmation>.Fail("invalid_reset_token", "Reset token is invalid or expired.");
        }

        var now = DateTimeOffset.UtcNow;
        var tokenHash = HashResetToken(request.Token.Trim());
        var token = await passwordResetTokenStore.FindValidAsync(user.Id, tokenHash, now, cancellationToken);

        if (token is null)
        {
            return Result<PasswordResetConfirmation>.Fail("invalid_reset_token", "Reset token is invalid or expired.");
        }

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var passwordHash = HashPassword(request.NewPassword, salt);

        await userStore.UpdatePasswordAsync(
            user.Id,
            Convert.ToBase64String(passwordHash),
            Convert.ToBase64String(salt),
            cancellationToken);

        await passwordResetTokenStore.MarkUsedAsync(token.Id, now, cancellationToken);

        return Result<PasswordResetConfirmation>.Ok(new PasswordResetConfirmation("Password updated. You can login again."));
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email) =>
        email.Length <= 160 &&
        email.Contains('@', StringComparison.Ordinal) &&
        email.Contains('.', StringComparison.Ordinal);

    private static byte[] HashPassword(string password, byte[] salt) =>
        Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashSize);

    private static bool VerifyPassword(string password, AuthUser user)
    {
        var salt = Convert.FromBase64String(user.PasswordSalt);
        var expectedHash = Convert.FromBase64String(user.PasswordHash);
        var attemptedHash = HashPassword(password, salt);

        return CryptographicOperations.FixedTimeEquals(expectedHash, attemptedHash);
    }

    private static string HashResetToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

    private string BuildResetUrl(string email, string token)
    {
        var baseUrl = string.IsNullOrWhiteSpace(flowOptions.FrontendBaseUrl)
            ? "http://127.0.0.1:5173"
            : flowOptions.FrontendBaseUrl.TrimEnd('/');

        return $"{baseUrl}/?authResetEmail={Uri.EscapeDataString(email)}&authResetToken={Uri.EscapeDataString(token)}";
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
