namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record GitHubUserProfile(
    string Login,
    string? Name,
    string HtmlUrl,
    string? Bio,
    string? Company,
    string? Location,
    int PublicRepos,
    int Followers);
