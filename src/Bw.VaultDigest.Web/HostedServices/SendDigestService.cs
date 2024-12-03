using Bw.VaultDigest.Web.Options;
using Bw.VaultDigest.Web.Requests;
using MediatR;
using Microsoft.Extensions.Options;
using NCrontab;

namespace Bw.VaultDigest.Web.HostedServices;

public class SendDigestService(ILogger<SendDigestService> logger, IOptions<ScheduleOptions> options, IMediator mediator) 
    : ScheduledServiceBase(CrontabSchedule.Parse(options.Value.SendDigest), true, logger)
{
    protected override Task RunAsync(CancellationToken cancellationToken)
        => mediator.Send(new SendDigestCommand(), cancellationToken);
}