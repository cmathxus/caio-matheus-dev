namespace CaioMatheusDev.Api.Domain.Portfolio;

public sealed record Profile(
    string Name,
    string Role,
    string Location,
    string SummaryPt,
    string SummaryEn,
    string GitHubUrl,
    string LinkedInUrl,
    string[] Highlights);
