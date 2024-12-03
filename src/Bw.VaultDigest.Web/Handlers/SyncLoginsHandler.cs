using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.Requests;
using MediatR;

namespace Bw.VaultDigest.Web.Handlers;

public class SyncLoginsHandler(
    ILogger<SyncLoginsHandler> logger,
    MetricsFactory metricsFactory,
    ILoginProviderAdapter adapter,
    ISyncSetRepository repository)
    : IRequestHandler<SyncLoginsCommand>
{
    public async Task Handle(SyncLoginsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncLoginsHandler started");
        using var _ = metricsFactory.CreateDurationMetric("sync-logins.duration");

        var set = await adapter.GetLogins();
        await repository.AddSyncSet(set);
        
        logger.LogInformation("SyncLoginsHandler completed");
    }
}