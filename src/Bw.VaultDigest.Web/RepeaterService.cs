using Bw.VaultDigest.Web.Services;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Web;

public class Schedule
{
    public required string Cron { get; init; }
}

public class RepeaterService(ILogger<RepeaterService> logger, IOptions<Schedule> options, DigestService digestSvc)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cron = CrontabSchedule.Parse(options.Value.Cron);
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Digest schedule running");
            await digestSvc.CreateDigest();
            var next = cron.GetNextOccurrence(DateTime.Now);
            await Task.Delay(next - DateTime.Now, stoppingToken);
        }
    }
}