using CaioMatheusDev.Api.Application.Common;
using CaioMatheusDev.Api.Domain.Integrations;

namespace CaioMatheusDev.Api.Application.Interfaces;

public interface IStatusCheckService
{
    Task<Result<StatusSummary>> GetProjectStatusAsync(CancellationToken cancellationToken = default);

    Task<Result<StatusCheck>> CheckUrlAsync(string url, CancellationToken cancellationToken = default);
}
