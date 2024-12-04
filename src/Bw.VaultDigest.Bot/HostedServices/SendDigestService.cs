using Bw.VaultDigest.Bot.Options;
using Bw.VaultDigest.Bot.Requests;
using MediatR;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Bot.HostedServices;

public class SendDigestService(ILogger<SendDigestService> logger, IOptions<ScheduleOptions> options, IMediator mediator) 
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SendDigest), true, logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => mediator.Send(new SendDigestCommand(), cancellationToken);
}