namespace CaioMatheusDev.Api.Infrastructure.Email;

public sealed class ResendOptions
{
    public string ApiKey { get; set; } = string.Empty;

    public string From { get; set; } = "Caio Matheus Dev <noreply@yahub.com.br>";
}
