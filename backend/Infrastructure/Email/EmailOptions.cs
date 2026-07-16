namespace CaioMatheusDev.Api.Infrastructure.Email;

public sealed class EmailOptions
{
    public string From { get; set; } = string.Empty;

    public string SmtpHost { get; set; } = "smtp.gmail.com";

    public int SmtpPort { get; set; } = 587;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool EnableSsl { get; set; } = true;

    public string FrontendBaseUrl { get; set; } = "http://127.0.0.1:5173";

    public string ResetGifUrl { get; set; } = "https://cmathxus.github.io/caio-matheus-dev/kirito-reset.gif";
}

