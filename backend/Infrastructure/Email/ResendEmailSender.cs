using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Auth;
using Microsoft.Extensions.Options;

namespace CaioMatheusDev.Api.Infrastructure.Email;

public sealed class ResendEmailSender(
    HttpClient httpClient,
    IOptions<ResendOptions> options,
    IOptions<EmailOptions> emailOptions) : IEmailSender
{
    private readonly ResendOptions resendOptions = options.Value;
    private readonly EmailOptions emailSettings = emailOptions.Value;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(resendOptions.ApiKey) &&
        !string.IsNullOrWhiteSpace(resendOptions.From);

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

        var email = PasswordResetEmailTemplate.Build(user, resetUrl, expiresAt, emailSettings.ResetGifUrl);
        using var request = new HttpRequestMessage(HttpMethod.Post, "emails");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", resendOptions.ApiKey);
        request.Content = JsonContent.Create(new ResendEmailRequest(
            resendOptions.From,
            [user.Email],
            email.Subject,
            email.Html,
            email.Text));

        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Resend rejected the password reset email. Status: {(int)response.StatusCode}. Body: {responseBody}");
    }

    private sealed record ResendEmailRequest(
        [property: JsonPropertyName("from")] string From,
        [property: JsonPropertyName("to")] string[] To,
        [property: JsonPropertyName("subject")] string Subject,
        [property: JsonPropertyName("html")] string Html,
        [property: JsonPropertyName("text")] string Text);
}
