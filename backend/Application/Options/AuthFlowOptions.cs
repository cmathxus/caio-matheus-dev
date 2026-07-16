namespace CaioMatheusDev.Api.Application.Options;

public sealed class AuthFlowOptions
{
    public string FrontendBaseUrl { get; set; } = "http://127.0.0.1:5173";

    public int PasswordResetMinutes { get; set; } = 15;
}

