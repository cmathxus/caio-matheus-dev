using System.Net;
using CaioMatheusDev.Api.Domain.Auth;

namespace CaioMatheusDev.Api.Infrastructure.Email;

public sealed record PasswordResetEmailContent(
    string Subject,
    string Text,
    string Html);

public static class PasswordResetEmailTemplate
{
    public const string Subject = "Recuperação de senha - Auth Lab";

    public static PasswordResetEmailContent Build(
        AuthUserProfile user,
        string resetUrl,
        DateTimeOffset expiresAt,
        string? imageSource)
    {
        var encodedName = WebUtility.HtmlEncode(user.Name);
        var encodedResetUrl = WebUtility.HtmlEncode(resetUrl);
        var imageRow = string.IsNullOrWhiteSpace(imageSource)
            ? string.Empty
            : $"""
              <tr>
                <td>
                  <img src="{WebUtility.HtmlEncode(imageSource)}" alt="Kirito olhando para a neve" width="620" style="display:block;width:100%;max-height:260px;object-fit:cover;background:#111827;">
                </td>
              </tr>
              """;

        var text = $"""
        Olá, {user.Name}.

        Recebemos uma solicitação para redefinir a senha do Auth Lab.

        Clique no link abaixo e informe sua nova senha:
        {resetUrl}

        Esse link expira em {expiresAt:dd/MM/yyyy HH:mm} UTC.

        Se você não solicitou isso, ignore este email.
        """;

        var html = $"""
        <!doctype html>
        <html lang="pt-BR">
          <body style="margin:0;background:#f4f4f5;padding:24px;font-family:Arial,Helvetica,sans-serif;color:#111827;">
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:14px;overflow:hidden;">
              {imageRow}
              <tr>
                <td style="padding:28px;">
                  <p style="margin:0 0 10px;color:#6b7280;font-size:13px;letter-spacing:.04em;text-transform:uppercase;">Auth Lab</p>
                  <h1 style="margin:0 0 12px;font-size:26px;line-height:1.2;color:#111827;">Redefinir senha</h1>
                  <p style="margin:0 0 18px;font-size:16px;line-height:1.6;color:#374151;">
                    Olá, {encodedName}. Recebemos uma solicitação para redefinir sua senha.
                  </p>
                  <p style="margin:0 0 24px;font-size:15px;line-height:1.6;color:#4b5563;">
                    Clique no botão abaixo, informe uma nova senha e o Auth Lab faz login automaticamente depois.
                  </p>
                  <a href="{encodedResetUrl}" style="display:inline-block;background:#111827;color:#ffffff;text-decoration:none;border-radius:10px;padding:13px 18px;font-weight:700;">
                    Redefinir minha senha
                  </a>
                  <p style="margin:24px 0 0;font-size:13px;line-height:1.6;color:#6b7280;">
                    Esse link expira em {expiresAt:dd/MM/yyyy HH:mm} UTC. Se você não solicitou isso, ignore este email.
                  </p>
                </td>
              </tr>
            </table>
          </body>
        </html>
        """;

        return new PasswordResetEmailContent(Subject, text, html);
    }
}
