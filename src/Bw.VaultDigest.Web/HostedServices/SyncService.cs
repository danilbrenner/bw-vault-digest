using Bw.VaultDigest.Web.Requests;
using MediatR;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Web.HostedServices;

public class Schedule
{
    public required string SyncLogins { get; init; }
}

public class SyncService(ILogger<SyncService> logger, IOptions<Schedule> options, IMediator mediator)
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SyncLogins), logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => mediator.Send(new SyncLoginsCommand(), cancellationToken);
}