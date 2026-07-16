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

        using var message = new MailMessage
        {
            From = new MailAddress(emailOptions.From, "Caio Matheus Dev"),
            Subject = "Recuperação de senha - Auth Lab",
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(user.Email, user.Name));
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
            BuildPlainTextBody(user, resetUrl, expiresAt),
            null,
            MediaTypeNames.Text.Plain));

        var htmlView = AlternateView.CreateAlternateViewFromString(
            BuildHtmlBody(user, resetUrl, expiresAt),
            null,
            MediaTypeNames.Text.Html);

        if (File.Exists(resetGifPath))
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
            Credentials = new NetworkCredential(emailOptions.Username, emailOptions.Password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }

    private static string BuildPlainTextBody(AuthUserProfile user, string resetUrl, DateTimeOffset expiresAt) =>
        $"""
        Olá, {user.Name}.

        Recebemos uma solicitação para redefinir a senha do Auth Lab.

        Clique no link abaixo e informe sua nova senha:
        {resetUrl}

        Esse link expira em {expiresAt:dd/MM/yyyy HH:mm} UTC.

        Se você não solicitou isso, ignore este email.
        """;

    private static string BuildHtmlBody(AuthUserProfile user, string resetUrl, DateTimeOffset expiresAt) =>
        $$"""
        <!doctype html>
        <html lang="pt-BR">
          <body style="margin:0;background:#f4f4f5;padding:24px;font-family:Arial,Helvetica,sans-serif;color:#111827;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:14px;overflow:hidden;">
              <tr>
                <td>
                  <img src="cid:kirito-reset" alt="Kirito olhando para a neve" width="620" style="display:block;width:100%;max-height:260px;object-fit:cover;background:#111827;">
                </td>
              </tr>
              <tr>
                <td style="padding:28px;">
                  <p style="margin:0 0 10px;color:#6b7280;font-size:13px;letter-spacing:.04em;text-transform:uppercase;">Auth Lab</p>
                  <h1 style="margin:0 0 12px;font-size:26px;line-height:1.2;color:#111827;">Redefinir senha</h1>
                  <p style="margin:0 0 18px;font-size:16px;line-height:1.6;color:#374151;">
                    Olá, {{WebUtility.HtmlEncode(user.Name)}}. Recebemos uma solicitação para redefinir sua senha.
                  </p>
                  <p style="margin:0 0 24px;font-size:15px;line-height:1.6;color:#4b5563;">
                    Clique no botão abaixo, informe uma nova senha e o Auth Lab faz login automaticamente depois.
                  </p>
                  <a href="{{WebUtility.HtmlEncode(resetUrl)}}" style="display:inline-block;background:#111827;color:#ffffff;text-decoration:none;border-radius:10px;padding:13px 18px;font-weight:700;">
                    Redefinir minha senha
                  </a>
                  <p style="margin:24px 0 0;font-size:13px;line-height:1.6;color:#6b7280;">
                    Esse link expira em {{expiresAt:dd/MM/yyyy HH:mm}} UTC. Se você não solicitou isso, ignore este email.
                  </p>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;
}

