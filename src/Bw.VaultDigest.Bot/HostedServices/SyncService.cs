using Bw.VaultDigest.Application.Requests;
using Bw.VaultDigest.Bot.Options;
using MediatR;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Bot.HostedServices;

public class SyncService(ILogger<SyncService> logger, IOptions<ScheduleOptions> options, IMediator mediator)
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SyncLogins), true, logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => mediator.Send(new SyncLoginsCommand(), cancellationToken);
}