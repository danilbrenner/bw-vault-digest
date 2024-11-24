using NCrontab;

namespace Bw.VaultDigest.Web.HostedServices;

public abstract class ScheduledServiceBase(CrontabSchedule cron, ILogger logger) : BackgroundService
{
    protected abstract Task RunAsync(CancellationToken cancellationToken);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("{ScheduledServiceType}: Is active", GetType().Name);
            await RunAsync(stoppingToken);
            var next = cron.GetNextOccurrence(DateTime.Now);
            logger.LogInformation("{ScheduledServiceType}: Scheduling next run for {Next}", GetType().Name, next);
            await Task.Delay(next - DateTime.Now, stoppingToken);
        }
    }
}