using NCrontab;

namespace Bw.VaultDigest.Bot.HostedServices;

public abstract class ScheduledServiceBase(CrontabSchedule cron, bool skipOnStartup, ILogger logger) : BackgroundService
{
    private DateTime? _next;
    
    protected abstract Task RunAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{ScheduledServiceType}: Is active", GetType().Name);
            if(_next is not null || !skipOnStartup)
                try
                {
                    await RunAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error while running scheduled service {Type}", GetType().Name);
                }
            _next = cron.GetNextOccurrence(DateTime.Now);
            logger.LogInformation("{ScheduledServiceType}: Scheduling next run for {Next}", GetType().Name, _next.Value);
            await Task.Delay(_next.Value - DateTime.Now, stoppingToken);
        }
    }
}