using System.Globalization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.Extensions.Options;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<AuthLabOptions> options) : IJwtTokenService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly AuthLabOptions authOptions = options.Value;
    private const string SubjectClaim = "sub";
    private const string JwtIdClaim = "jti";
    private const string EmailClaim = "email";
    private const string NameClaim = "name";
    private const string IssuerClaim = "iss";
    private const string AudienceClaim = "aud";
    private const string IssuedAtClaim = "iat";
    private const string ExpiresAtClaim = "exp";

    public AuthSession CreateSession(AuthUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(authOptions.ExpiresInMinutes);
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        var payload = new Dictionary<string, object>
        {
            [SubjectClaim] = user.Id.ToString(),
            [JwtIdClaim] = Guid.NewGuid().ToString("N"),
            [EmailClaim] = user.Email,
            [NameClaim] = user.Name,
            [IssuerClaim] = authOptions.Issuer,
            [AudienceClaim] = authOptions.Audience,
            [IssuedAtClaim] = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            [ExpiresAtClaim] = expiresAt.ToUnixTimeSeconds(),
            ["scope"] = "auth-lab:read"
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header, SerializerOptions));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload, SerializerOptions));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = Sign(unsignedToken);
        var token = $"{unsignedToken}.{signature}";
        var profile = new AuthUserProfile(user.Id, user.Name, user.Email, user.CreatedAt);

        return new AuthSession(token, "Bearer", expiresAt, profile);
    }

    public Result<AuthTokenPayload> Validate(string authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader) ||
            !authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Result<AuthTokenPayload>.Fail("missing_token", "Send the token using the Authorization: Bearer header.");
        }

        var token = authorizationHeader["Bearer ".Length..].Trim();
        var parts = token.Split('.');

        if (parts.Length != 3)
        {
            return Result<AuthTokenPayload>.Fail("invalid_token", "JWT format is invalid.");
        }

        var unsignedToken = $"{parts[0]}.{parts[1]}";
        var expectedSignature = Sign(unsignedToken);

        if (!FixedTimeEquals(expectedSignature, parts[2]))
        {
            return Result<AuthTokenPayload>.Fail("invalid_signature", "JWT signature is invalid.");
        }

        try
        {
            using var payloadDocument = JsonDocument.Parse(Base64UrlDecode(parts[1]));
            var payload = payloadDocument.RootElement;
            var expiresAt = DateTimeOffset.FromUnixTimeSeconds(payload.GetProperty(ExpiresAtClaim).GetInt64());

            if (expiresAt <= DateTimeOffset.UtcNow)
            {
                return Result<AuthTokenPayload>.Fail("expired_token", "JWT expired. Login again.");
            }

            var issuer = payload.GetProperty(IssuerClaim).GetString();
            var audience = payload.GetProperty(AudienceClaim).GetString();

            if (issuer != authOptions.Issuer || audience != authOptions.Audience)
            {
                return Result<AuthTokenPayload>.Fail("invalid_token_context", "JWT issuer or audience is invalid.");
            }

            var userId = Guid.Parse(payload.GetProperty(SubjectClaim).GetString()!);
            var name = payload.GetProperty(NameClaim).GetString()!;
            var email = payload.GetProperty(EmailClaim).GetString()!;
            var scope = payload.TryGetProperty("scope", out var scopeElement)
                ? scopeElement.GetString() ?? string.Empty
                : string.Empty;
            var claims = new[]
            {
                $"{ClaimTypes.NameIdentifier}: {userId}",
                $"{ClaimTypes.Name}: {name}",
                $"{ClaimTypes.Email}: {email}",
                $"scope: {scope}",
                $"exp: {expiresAt.ToString("u", CultureInfo.InvariantCulture)}"
            };

            return Result<AuthTokenPayload>.Ok(new AuthTokenPayload(userId, name, email, expiresAt, claims));
        }
        catch
        {
            return Result<AuthTokenPayload>.Fail("invalid_payload", "JWT payload could not be read.");
        }
    }

    private string Sign(string value)
    {
        var key = Encoding.UTF8.GetBytes(authOptions.Secret);
        using var hmac = new HMACSHA256(key);
        var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(value));

        return Base64UrlEncode(signature);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = base64.Length % 4;

        if (padding > 0)
        {
            base64 = base64.PadRight(base64.Length + 4 - padding, '=');
        }

        return Convert.FromBase64String(base64);
    }

    private static bool FixedTimeEquals(string expected, string actual)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return expectedBytes.Length == actualBytes.Length &&
               CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
