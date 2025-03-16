using Bw.VaultBot.Application.Requests;
using Bw.VaultBot.Options;
using MediatR;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultBot.HostedServices;

public class SendDigestService(ILogger<SendDigestService> logger, IOptions<ScheduleOptions> options, IMediator mediator) 
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SendDigest), true, logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => mediator.Send(new SendStatisticsCommand(), cancellationToken);
}