using System.Globalization;
using System.Text.Json.Serialization;
using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Infrastructure.Http;

public sealed class IntegrationLabService(IHttpClientFactory httpClientFactory) : IIntegrationLabService
{
    private static readonly HashSet<string> AllowedDnsRecordTypes = ["A", "AAAA", "CNAME", "MX", "TXT"];

    public async Task<Result<GitHubUserProfile>> GetGitHubUserAsync(
        string username,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result<GitHubUserProfile>.Fail("invalid_username", "GitHub username is required.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("github");
            var user = await client.GetFromJsonAsync<GitHubUserApiResponse>(
                $"users/{Uri.EscapeDataString(username.Trim())}",
                cancellationToken);

            return user is null
                ? Result<GitHubUserProfile>.Fail("github_user_not_found", "GitHub user not found.")
                : Result<GitHubUserProfile>.Ok(user.ToDomain());
        }
        catch
        {
            return Result<GitHubUserProfile>.Fail("github_unavailable", "GitHub is unavailable right now.");
        }
    }

    public async Task<Result<IReadOnlyCollection<NuGetPackageResult>>> SearchNuGetPackagesAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result<IReadOnlyCollection<NuGetPackageResult>>.Fail(
                "invalid_query",
                "Package query is required.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("default");
            var response = await client.GetFromJsonAsync<NuGetSearchApiResponse>(
                $"https://azuresearch-usnc.nuget.org/query?q={Uri.EscapeDataString(query.Trim())}&take=5",
                cancellationToken);

            var packages = response?.Data
                .Select(package => package.ToDomain())
                .ToArray() ?? [];

            return Result<IReadOnlyCollection<NuGetPackageResult>>.Ok(packages);
        }
        catch
        {
            return Result<IReadOnlyCollection<NuGetPackageResult>>.Fail(
                "nuget_unavailable",
                "NuGet search is unavailable right now.");
        }
    }

    public async Task<Result<DnsLookupResult>> ResolveDnsAsync(
        string domain,
        string recordType,
        CancellationToken cancellationToken = default)
    {
        var normalizedDomain = domain.Trim().ToLowerInvariant();
        var normalizedType = string.IsNullOrWhiteSpace(recordType)
            ? "A"
            : recordType.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(normalizedDomain) || normalizedDomain.Contains(" "))
        {
            return Result<DnsLookupResult>.Fail("invalid_domain", "Domain is invalid.");
        }

        if (!AllowedDnsRecordTypes.Contains(normalizedType))
        {
            return Result<DnsLookupResult>.Fail("invalid_record_type", "Use A, AAAA, CNAME, MX or TXT.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("default");
            var response = await client.GetFromJsonAsync<GoogleDnsApiResponse>(
                $"https://dns.google/resolve?name={Uri.EscapeDataString(normalizedDomain)}&type={normalizedType}",
                cancellationToken);

            var answers = response?.Answer?
                .Select(answer => answer.ToDomain())
                .ToArray() ?? [];

            return Result<DnsLookupResult>.Ok(new DnsLookupResult(normalizedDomain, normalizedType, answers));
        }
        catch
        {
            return Result<DnsLookupResult>.Fail("dns_unavailable", "DNS resolver is unavailable right now.");
        }
    }

    public async Task<Result<IReadOnlyCollection<AnimeSearchResult>>> SearchAnimeAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return Result<IReadOnlyCollection<AnimeSearchResult>>.Fail(
                "invalid_anime_query",
                "Anime query is required.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("default");
            var response = await client.GetFromJsonAsync<KitsuAnimeSearchApiResponse>(
                $"https://kitsu.io/api/edge/anime?filter%5Btext%5D={Uri.EscapeDataString(query.Trim())}&page%5Blimit%5D=5",
                cancellationToken);

            var anime = response?.Data
                .Select(item => item.ToDomain())
                .ToArray() ?? [];

            return Result<IReadOnlyCollection<AnimeSearchResult>>.Ok(anime);
        }
        catch
        {
            return Result<IReadOnlyCollection<AnimeSearchResult>>.Fail(
                "anime_unavailable",
                "Anime search is unavailable right now.");
        }
    }

    public async Task<Result<WeatherSnapshot>> GetWeatherAsync(
        string city,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return Result<WeatherSnapshot>.Fail("invalid_city", "City is required.");
        }

        if (latitude is < -90 or > 90 || longitude is < -180 or > 180)
        {
            return Result<WeatherSnapshot>.Fail("invalid_coordinates", "Coordinates are invalid.");
        }

        try
        {
            var client = httpClientFactory.CreateClient("default");
            var url = FormattableString.Invariant(
                $"https://api.open-meteo.com/v1/forecast?latitude={latitude}&longitude={longitude}&current=temperature_2m,wind_speed_10m,weather_code&timezone=America%2FSao_Paulo");
            var response = await client.GetFromJsonAsync<OpenMeteoApiResponse>(url, cancellationToken);

            return response?.Current is null
                ? Result<WeatherSnapshot>.Fail("weather_not_found", "Weather data was not found.")
                : Result<WeatherSnapshot>.Ok(response.Current.ToDomain(city.Trim(), latitude, longitude));
        }
        catch
        {
            return Result<WeatherSnapshot>.Fail("weather_unavailable", "Weather service is unavailable right now.");
        }
    }
}

public sealed record GitHubUserApiResponse(
    string Login,
    string? Name,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    string? Bio,
    string? Company,
    string? Location,
    [property: JsonPropertyName("public_repos")] int PublicRepos,
    int Followers)
{
    public GitHubUserProfile ToDomain() => new(
        Login,
        Name,
        HtmlUrl,
        Bio,
        Company,
        Location,
        PublicRepos,
        Followers);
}

public sealed record NuGetSearchApiResponse(NuGetPackageApiResponse[] Data);

public sealed record NuGetPackageApiResponse(
    string Id,
    string Version,
    string? Description,
    string? ProjectUrl,
    long TotalDownloads)
{
    public NuGetPackageResult ToDomain() => new(Id, Version, Description, ProjectUrl, TotalDownloads);
}

public sealed record GoogleDnsApiResponse(GoogleDnsAnswerApiResponse[]? Answer);

public sealed record GoogleDnsAnswerApiResponse(string Name, int Type, int Ttl, string Data)
{
    public DnsAnswer ToDomain() => new(Name, Type.ToString(), Ttl, Data);
}

public sealed record KitsuAnimeSearchApiResponse(KitsuAnimeApiResponse[] Data);

public sealed record KitsuAnimeApiResponse(
    string Id,
    KitsuAnimeLinks Links,
    KitsuAnimeAttributes Attributes)
{
    public AnimeSearchResult ToDomain() => new(
        Attributes.CanonicalTitle,
        Attributes.Titles?.Japanese,
        Attributes.Subtype,
        Attributes.EpisodeCount,
        double.TryParse(Attributes.AverageRating, CultureInfo.InvariantCulture, out var score) ? score : null,
        Attributes.Status,
        Links.Self,
        Attributes.PosterImage?.Small,
        Attributes.Synopsis);
}

public sealed record KitsuAnimeLinks(string Self);

public sealed record KitsuAnimeAttributes(
    string Slug,
    string Synopsis,
    KitsuAnimeTitles? Titles,
    string CanonicalTitle,
    string? AverageRating,
    string? Subtype,
    string? Status,
    int? EpisodeCount,
    KitsuAnimePosterImage? PosterImage);

public sealed record KitsuAnimeTitles([property: JsonPropertyName("ja_jp")] string? Japanese);

public sealed record KitsuAnimePosterImage(string? Small);

public sealed record OpenMeteoApiResponse(OpenMeteoCurrentApiResponse? Current);

public sealed record OpenMeteoCurrentApiResponse(
    string Time,
    [property: JsonPropertyName("temperature_2m")] double TemperatureCelsius,
    [property: JsonPropertyName("wind_speed_10m")] double WindSpeedKmH,
    [property: JsonPropertyName("weather_code")] int WeatherCode)
{
    public WeatherSnapshot ToDomain(string city, double latitude, double longitude)
    {
        var fetchedAt = DateTimeOffset.TryParse(
            Time,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal,
            out var parsed)
            ? parsed
            : DateTimeOffset.UtcNow;

        return new WeatherSnapshot(city, latitude, longitude, TemperatureCelsius, WindSpeedKmH, WeatherCode, fetchedAt);
    }
}
