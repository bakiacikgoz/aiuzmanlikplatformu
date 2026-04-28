namespace AiEducation.Api.Features.Leagues;

public sealed class LeagueRolloverHostedService(
    IServiceScopeFactory scopeFactory,
    ILogger<LeagueRolloverHostedService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(Interval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var leagueService = scope.ServiceProvider.GetRequiredService<LeagueService>();
            await leagueService.RolloverIfNeededAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "League rollover background check failed.");
        }
    }
}
