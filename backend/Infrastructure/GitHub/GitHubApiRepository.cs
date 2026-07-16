using System.Text.Json.Serialization;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Infrastructure.GitHub;

public sealed record GitHubApiRepository(
    string Name,
    [property: JsonPropertyName("html_url")] string HtmlUrl,
    string? Description,
    string? Language,
    [property: JsonPropertyName("stargazers_count")] int Stars,
    [property: JsonPropertyName("forks_count")] int Forks,
    [property: JsonPropertyName("updated_at")] DateTimeOffset UpdatedAt)
{
    public GitHubRepository ToDomain() => new(
        Name,
        HtmlUrl,
        Description,
        Language,
        Stars,
        Forks,
        UpdatedAt);
}
