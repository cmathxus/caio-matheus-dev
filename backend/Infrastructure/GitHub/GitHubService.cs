using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;
using Microsoft.Extensions.Caching.Memory;

namespace CaioMatheusDev.Api.Infrastructure.GitHub;

public sealed class GitHubService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache) : IGitHubService
{
    private const string CacheKey = "github:repos";

    public async Task<Result<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>> GetRepositoriesAsync(
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(CacheKey, out GitHubRepository[]? cached) && cached is not null)
        {
            return Result<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>.Ok(
                new IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>(cached, true, DateTimeOffset.UtcNow));
        }

        try
        {
            var repositories = await LoadRepositoriesAsync(cancellationToken);
            cache.Set(CacheKey, repositories, TimeSpan.FromMinutes(10));

            return Result<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>.Ok(
                new IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>(repositories, false, DateTimeOffset.UtcNow));
        }
        catch
        {
            return Result<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>.Fail(
                "github_unavailable",
                "Could not load GitHub repositories right now.");
        }
    }

    public async Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        var repositories = await LoadRepositoriesAsync(cancellationToken);
        cache.Set(CacheKey, repositories, TimeSpan.FromMinutes(12));
    }

    private async Task<GitHubRepository[]> LoadRepositoriesAsync(CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("github");
        var response = await client.GetFromJsonAsync<GitHubApiRepository[]>(
            "users/cmathxus/repos?sort=updated&per_page=8",
            cancellationToken);

        return response?.Select(repository => repository.ToDomain()).ToArray() ?? [];
    }
}
