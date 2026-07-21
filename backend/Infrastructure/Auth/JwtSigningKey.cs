using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CaioMatheusDev.Api.Infrastructure.Auth;

internal static class JwtSigningKey
{
    public static SymmetricSecurityKey FromSecret(string secret)
    {
        var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(secret));

        return new SymmetricSecurityKey(keyBytes);
    }
}
