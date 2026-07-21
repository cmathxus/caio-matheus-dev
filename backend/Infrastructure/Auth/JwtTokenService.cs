using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<AuthLabOptions> options) : IJwtTokenService
{
    private readonly AuthLabOptions authOptions = options.Value;

    public AuthSession CreateSession(AuthUser user)
    {
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(authOptions.ExpiresInMinutes);
        var signingKey = JwtSigningKey.FromSecret(authOptions.Secret);
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim("scope", "auth-lab:read")
        };
        var securityToken = new JwtSecurityToken(
            issuer: authOptions.Issuer,
            audience: authOptions.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);
        var token = new JwtSecurityTokenHandler().WriteToken(securityToken);
        var profile = new AuthUserProfile(user.Id, user.Name, user.Email, user.CreatedAt);

        return new AuthSession(token, "Bearer", expiresAt, profile);
    }
}
