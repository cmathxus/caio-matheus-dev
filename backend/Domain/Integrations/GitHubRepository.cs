namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record GitHubRepository(
    string Name,
    string HtmlUrl,
    string? Description,
    string? Language,
    int Stars,
    int Forks,
    DateTimeOffset UpdatedAt);
