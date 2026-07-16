namespace CaioMatheusDev.Api.Domain.Integrations;

public sealed record AnimeSearchResult(
    string Title,
    string? TitleJapanese,
    string? Type,
    int? Episodes,
    double? Score,
    string? Status,
    string Url,
    string? ImageUrl,
    string? Synopsis);
