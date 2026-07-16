using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Application.Interfaces;
using CaioMatheusDev.Api.Domain.Integrations;
using Microsoft.AspNetCore.Mvc;

namespace CaioMatheusDev.Api.Presentation.Controllers;

[Route("api")]
public sealed class IntegrationsController(
    IGitHubService gitHubService,
    IStatusCheckService statusCheckService) : ApiControllerBase
{
    [HttpGet("github/repos")]
    public async Task<ActionResult<ApiResponse<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>>> GetGitHubRepos(
        CancellationToken cancellationToken)
    {
        var result = await gitHubService.GetRepositoriesAsync(cancellationToken);
        return FromResult(result);
    }

    [HttpGet("status/projects")]
    public async Task<ActionResult<ApiResponse<StatusSummary>>> GetProjectStatus(
        CancellationToken cancellationToken)
    {
        var result = await statusCheckService.GetProjectStatusAsync(cancellationToken);
        return FromResult(result);
    }
}
