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
            logger.LogInformation("RepeaterService is active: Creating digest");

            using (logger.BeginScope(
                       new Dictionary<string, object> { { "RepeaterServiceExecutionId", Guid.NewGuid() } }))
            using (metricsFactory.CreateDurationMetric("repeater-run.duration"))
            {
                await digestSvc.CreateDigest();

                logger.LogInformation("Digest has been created");
            }

            var next = cron.GetNextOccurrence(DateTime.Now);

            logger.LogInformation("Scheduling next run for {Next}", next);

            await Task.Delay(next - DateTime.Now, stoppingToken);
        }
    }
}