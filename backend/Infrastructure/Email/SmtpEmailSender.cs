using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace CaioMatheusDev.Api.Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<EmailOptions> options,
    IWebHostEnvironment environment) : IEmailSender
{
    private readonly EmailOptions emailOptions = options.Value;
    private readonly string resetGifPath = Path.Combine(
        environment.ContentRootPath,
        "Infrastructure",
        "Email",
        "Assets",
        "kirito-reset.gif");

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(emailOptions.From) &&
        !string.IsNullOrWhiteSpace(emailOptions.SmtpHost) &&
        !string.IsNullOrWhiteSpace(emailOptions.Username) &&
        !string.IsNullOrWhiteSpace(emailOptions.Password);

    public async Task SendPasswordResetAsync(
        AuthUserProfile user,
        string resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
        {
            return;
        }

        var hasInlineGif = File.Exists(resetGifPath);
        var email = PasswordResetEmailTemplate.Build(
            user,
            resetUrl,
            expiresAt,
            hasInlineGif ? "cid:kirito-reset" : emailOptions.ResetGifUrl);

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.From, "Caio Matheus Dev"),
            Subject = email.Subject,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(user.Email, user.Name));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            email.Text,
            null,
            MediaTypeNames.Text.Plain));

        var htmlView = AlternateView.CreateAlternateViewFromString(
            email.Html,
            null,
            MediaTypeNames.Text.Html);

        if (hasInlineGif)
        {
            var gif = new LinkedResource(resetGifPath, "image/gif")
            {
                ContentId = "kirito-reset",
                TransferEncoding = TransferEncoding.Base64
            };

            htmlView.LinkedResources.Add(gif);
        }

        message.AlternateViews.Add(htmlView);

        using var client = new SmtpClient(emailOptions.SmtpHost, emailOptions.SmtpPort)
        {
            EnableSsl = emailOptions.EnableSsl,
            Credentials = new NetworkCredential(emailOptions.Username, emailOptions.Password),
            Timeout = 12_000
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
