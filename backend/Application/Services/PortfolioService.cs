using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Portfolio;
using CaioMatheusDev.Api.Infrastructure.Data;

namespace CaioMatheusDev.Api.Application.Services;

public sealed class PortfolioService : IPortfolioService
{
    public Profile GetProfile() => PortfolioData.Profile;

    public IReadOnlyCollection<Project> GetProjects() => PortfolioData.Projects;

    public IReadOnlyCollection<SkillGroup> GetSkills() => PortfolioData.Skills;

    public IReadOnlyCollection<ExperienceItem> GetExperience() => PortfolioData.Experience;

    public IReadOnlyCollection<Certification> GetCertifications() => PortfolioData.Certifications;
}
