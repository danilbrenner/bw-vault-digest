using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Model;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.Requests;
using MediatR;

namespace Bw.VaultDigest.Web.Handlers;

public class SyncLoginsHandler(
    IMediator mediator,
    ILogger<SyncLoginsHandler> logger,
    MetricsFactory metricsFactory,
    ILoginProviderAdapter adapter)
    : IRequestHandler<SyncLoginsCommand>
{
    public async Task Handle(SyncLoginsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncLoginsHandler started");
        LoginsSet? set;
        using (metricsFactory.CreateDurationMetric("sync-logins.duration"))
        {
            set = await adapter.GetLogins();
        }

        await mediator.Publish(new LoginsSyncedEvent(set), cancellationToken);
        logger.LogInformation("SyncLoginsHandler completed");
    }
}