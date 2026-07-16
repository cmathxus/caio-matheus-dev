namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record NuGetPackageResult(
    string Id,
    string Version,
    string? Description,
    string? ProjectUrl,
    long TotalDownloads);
