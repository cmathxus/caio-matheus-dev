using System.Diagnostics;
using System.Net;
using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;
using CaioMatheusDev.Api.Infrastructure.Data;

namespace CaioMatheusDev.Api.Infrastructure.Http;

public sealed class StatusCheckService(IHttpClientFactory httpClientFactory) : IStatusCheckService
{
    public async Task<Result<StatusSummary>> GetProjectStatusAsync(CancellationToken cancellationToken = default)
    {
        var client = httpClientFactory.CreateClient("default");
        var checks = await Task.WhenAll(PortfolioData.StatusTargets.Select(target =>
            CheckEndpointAsync(client, target, cancellationToken)));

        return Result<StatusSummary>.Ok(new StatusSummary(DateTimeOffset.UtcNow, checks));
    }

    public async Task<Result<StatusCheck>> CheckUrlAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return Result<StatusCheck>.Fail("invalid_url", "Use an absolute HTTP or HTTPS URL.");
        }

        var client = httpClientFactory.CreateClient("default");
        var check = await CheckEndpointAsync(client, new StatusTarget(uri.Host, uri.ToString()), cancellationToken);
        return Result<StatusCheck>.Ok(check);
    }

    private static async Task<StatusCheck> CheckEndpointAsync(
        HttpClient client,
        StatusTarget target,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, target.Url);
            using var response = await client.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);
            stopwatch.Stop();

            return new StatusCheck(
                target.Name,
                target.Url,
                response.StatusCode,
                response.IsSuccessStatusCode,
                stopwatch.ElapsedMilliseconds,
                DateTimeOffset.UtcNow);
        }
        catch
        {
            stopwatch.Stop();

            return new StatusCheck(
                target.Name,
                target.Url,
                HttpStatusCode.ServiceUnavailable,
                false,
                stopwatch.ElapsedMilliseconds,
                DateTimeOffset.UtcNow);
        }
    }
}
