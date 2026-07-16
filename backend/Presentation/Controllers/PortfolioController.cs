using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Portfolio;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Route("api")]
public sealed class PortfolioController(IPortfolioService portfolioService) : ApiControllerBase
{
    [HttpGet("profile")]
    public ActionResult<ApiResponse<Profile>> GetProfile() =>
        FromResult(Result<Profile>.Ok(portfolioService.GetProfile()));

    [HttpGet("projects")]
    public ActionResult<ApiResponse<IReadOnlyCollection<Project>>> GetProjects() =>
        FromResult(Result<IReadOnlyCollection<Project>>.Ok(portfolioService.GetProjects()));

    [HttpGet("skills")]
    public ActionResult<ApiResponse<IReadOnlyCollection<SkillGroup>>> GetSkills() =>
        FromResult(Result<IReadOnlyCollection<SkillGroup>>.Ok(portfolioService.GetSkills()));

    [HttpGet("experience")]
    public ActionResult<ApiResponse<IReadOnlyCollection<ExperienceItem>>> GetExperience() =>
        FromResult(Result<IReadOnlyCollection<ExperienceItem>>.Ok(portfolioService.GetExperience()));

    [HttpGet("certifications")]
    public ActionResult<ApiResponse<IReadOnlyCollection<Certification>>> GetCertifications() =>
        FromResult(Result<IReadOnlyCollection<Certification>>.Ok(portfolioService.GetCertifications()));
}
