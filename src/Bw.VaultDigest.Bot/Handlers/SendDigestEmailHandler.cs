using Bw.VaultDigest.Bot.Options;
using Bw.VaultDigest.Bot.Requests;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Telemetry;
using MediatR;
using Microsoft.Extensions.Options;

namespace Bw.VaultDigest.Bot.Handlers;

public class SendDigestEmailHandler(
    MetricsFactory metricsFactory,
    IEmailNotifier notifier,
    IOptions<EmailContentOptions> contentOptions,
    ILogger<SendDigestEmailHandler> logger,
    ISyncSetRepository repository)
    : IRequestHandler<SendDigestCommand, Unit>
{
    public async Task<Unit> Handle(SendDigestCommand command, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending digest");
        using var _ = metricsFactory.CreateDurationMetric("send-digest.duration");

        var set = await repository.GetLatestSyncSet();

        if (set is null)
        {
            logger.LogError("No synchronization set was found to create the digest email");
            return;
        }

        await notifier.SendDigest(set,
            new SendDigestEmailSettings(
                contentOptions.Value.To, contentOptions.Value.Title, contentOptions.Value.Template));

        logger.LogInformation("Digest sent");
        return Unit.Value;
    }
}