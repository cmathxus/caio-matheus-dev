using CaioMatheusDev.Api.Domain.Portfolio;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IPortfolioService
{
    Profile GetProfile();

    IReadOnlyCollection<Project> GetProjects();

    IReadOnlyCollection<SkillGroup> GetSkills();

    IReadOnlyCollection<ExperienceItem> GetExperience();

    IReadOnlyCollection<Certification> GetCertifications();
}
