using CaioMatheusDev.Api.Application.Interfaces;

namespace CaioMatheusDev.Api.Infrastructure.Workers;

public sealed class PortfolioRefreshWorker(
    IGitHubService gitHubService,
    ILogger<PortfolioRefreshWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await gitHubService.RefreshCacheAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "GitHub refresh failed.");
            }

            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
