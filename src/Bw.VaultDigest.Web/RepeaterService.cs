using System.Diagnostics;
using System.Diagnostics.Metrics;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.Services;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Web;

public class Schedule
{
    public required string Cron { get; init; }
}

public class RepeaterService(
    ILogger<RepeaterService> logger, 
    IOptions<Schedule> options, 
    DigestService digestSvc,
    MetricsFactory metricsFactory)
    : BackgroundService
{
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var cron = CrontabSchedule.Parse(options.Value.Cron);

        while (!stoppingToken.IsCancellationRequested)
        {
            using var _0 = logger.BeginScope(new Dictionary<string, object>{ { "RepeaterServiceExecutionId", Guid.NewGuid() } });
            using var _1 = metricsFactory.CreateDurationMetric("repeater-run.duration");
            
            logger.LogInformation("RepeaterService is active: Creating digest");

            await digestSvc.CreateDigest();

            logger.LogInformation("RepeaterService: Digest has been created");

            var next = cron.GetNextOccurrence(DateTime.Now);

            logger.LogInformation("Scheduling next run for {Next}", next);
            
            await Task.Delay(next - DateTime.Now, stoppingToken);
        }
    }
}