using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IGitHubService
{
    Task<Result<IntegrationEnvelope<IReadOnlyCollection<GitHubRepository>>>> GetRepositoriesAsync(
        CancellationToken cancellationToken = default);

    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
