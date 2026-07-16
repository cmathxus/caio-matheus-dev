namespace CaioMatheusDev.Api.Domain.Portfolio;

public sealed record Project(
    string Id,
    string Name,
    string SummaryPt,
    string SummaryEn,
    string Stack,
    string ImpactPt,
    string ImpactEn,
    string RepositoryUrl,
    bool Production);
