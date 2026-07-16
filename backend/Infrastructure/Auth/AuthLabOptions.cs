namespace CaioMatheusDev.Api.Infrastructure.Auth;

public sealed class AuthLabOptions
{
    public string Issuer { get; set; } = "caio-matheus-dev";

    public string Audience { get; set; } = "portfolio-auth-lab";

    public string Secret { get; set; } = "local-development-secret-change-me-please-32-chars";

    public int ExpiresInMinutes { get; set; } = 45;
}

