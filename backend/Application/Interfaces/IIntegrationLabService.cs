using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IIntegrationLabService
{
    Task<Result<GitHubUserProfile>> GetGitHubUserAsync(
        string username,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<NuGetPackageResult>>> SearchNuGetPackagesAsync(
        string query,
        CancellationToken cancellationToken = default);

    Task<Result<DnsLookupResult>> ResolveDnsAsync(
        string domain,
        string recordType,
        CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<AnimeSearchResult>>> SearchAnimeAsync(
        string query,
        CancellationToken cancellationToken = default);

    Task<Result<WeatherSnapshot>> GetWeatherAsync(
        string city,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}
