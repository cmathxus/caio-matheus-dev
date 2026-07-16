using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Route("api/lab")]
public sealed class LabController(
    IAddressLookupService addressLookupService,
    IStatusCheckService statusCheckService,
    IIntegrationLabService integrationLabService) : ApiControllerBase
{
    [HttpGet("cep/{cep}")]
    public async Task<ActionResult<ApiResponse<ViaCepResponse>>> LookupCep(
        string cep,
        CancellationToken cancellationToken)
    {
        var result = await addressLookupService.LookupCepAsync(cep, cancellationToken);
        return FromResult(result, ResolveCepFailureStatus(result.Error?.Code));
    }

    [HttpGet("http-check")]
    public async Task<ActionResult<ApiResponse<StatusCheck>>> CheckUrl(
        [FromQuery] string url,
        CancellationToken cancellationToken)
    {
        var result = await statusCheckService.CheckUrlAsync(url, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("github/{username}")]
    public async Task<ActionResult<ApiResponse<GitHubUserProfile>>> GetGitHubUser(
        string username,
        CancellationToken cancellationToken)
    {
        var result = await integrationLabService.GetGitHubUserAsync(username, cancellationToken);
        return FromResult(result, result.Error?.Code is "github_user_not_found"
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest);
    }

    [HttpGet("nuget")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<NuGetPackageResult>>>> SearchNuGet(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var result = await integrationLabService.SearchNuGetPackagesAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("dns")]
    public async Task<ActionResult<ApiResponse<DnsLookupResult>>> ResolveDns(
        [FromQuery] string domain,
        [FromQuery] string recordType,
        CancellationToken cancellationToken)
    {
        var result = await integrationLabService.ResolveDnsAsync(domain, recordType, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("anime")]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AnimeSearchResult>>>> SearchAnime(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var result = await integrationLabService.SearchAnimeAsync(query, cancellationToken);
        return FromResult(result);
    }

    [HttpGet("weather")]
    public async Task<ActionResult<ApiResponse<WeatherSnapshot>>> GetWeather(
        [FromQuery] string city,
        [FromQuery] double latitude,
        [FromQuery] double longitude,
        CancellationToken cancellationToken)
    {
        var result = await integrationLabService.GetWeatherAsync(city, latitude, longitude, cancellationToken);
        return FromResult(result);
    }

    private static int ResolveCepFailureStatus(string? errorCode) =>
        errorCode is "cep_not_found"
            ? StatusCodes.Status404NotFound
            : StatusCodes.Status400BadRequest;
}
