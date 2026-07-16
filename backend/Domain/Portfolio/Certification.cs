namespace CaioMatheusDev.Api.Domain.Portfolio;

public sealed record Certification(
    string Id,
    string NamePt,
    string NameEn,
    string Issuer,
    string SummaryPt,
    string SummaryEn,
    string ImageUrl,
    string CredentialUrl);
