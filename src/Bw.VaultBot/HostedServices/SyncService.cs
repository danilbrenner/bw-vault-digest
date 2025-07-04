using Bw.VaultBot.Options;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultBot.HostedServices;

public class SyncService(ILogger<SyncService> logger, IOptions<ScheduleOptions> options)
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SyncLogins), true, logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => Task.CompletedTask; // mediator.Send(new SyncLoginsCommand(), cancellationToken);
}