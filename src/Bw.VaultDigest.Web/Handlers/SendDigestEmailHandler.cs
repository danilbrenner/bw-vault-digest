using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.Requests;
using MediatR;

namespace Bw.VaultDigest.Web.Handlers;

public class SendDigestEmailHandler(
    MetricsFactory metricsFactory,
    IEmailTemplateLoader templateLoader,
    IEmailNotifier notifier,
    ILogger<SendDigestEmailHandler> logger,
    ISyncSetRepository repository) 
    : IRequestHandler<SendDigestCommand>
{
    public async Task Handle(SendDigestCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending digest");
        using var _ = metricsFactory.CreateDurationMetric("send-digest.duration");
        
        var set = await repository.GetLatestSyncSet();

        if (set is null)
        {
            logger.LogError("No synchronization set was found to create the digest email");
            return;
        }
        
        var template = await templateLoader.RenderMessage(
            set.Logins.Count,
            set.UserEmail,
            DateTime.Today);

        await notifier.SendEmail(
            template,
            [
                ("age-diagram", set.Logins.ToAgeSlices().ToDoughnutDiagram()),
                ("complexity-diagram", set.Logins.ToStrengthSlices().ToDoughnutDiagram())
            ]);

        logger.LogInformation("Digest sent");
    }
}