namespace CaioMatheusDev.Api.Domain.Portfolio;

public sealed record ExperienceItem(
    string Id,
    string ContextPt,
    string ContextEn,
    string RolePt,
    string RoleEn,
    string DescriptionPt,
    string DescriptionEn,
    string[] DetailsPt,
    string[] DetailsEn);
