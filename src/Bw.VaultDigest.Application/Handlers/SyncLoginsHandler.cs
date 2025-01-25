using Bw.VaultDigest.Application.Requests;
using Bw.VaultDigest.Data.Abstractions;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Telemetry;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bw.VaultDigest.Application.Handlers;

public class SyncLoginsHandler(
    ILogger<SyncLoginsHandler> logger,
    MetricsFactory metricsFactory,
    ILoginProviderAdapter adapter,
    ISyncSetRepository repository)
    : IRequestHandler<SyncLoginsCommand, Unit>
{
    public async Task<Unit> Handle(SyncLoginsCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("SyncLoginsHandler started");
        using var _ = metricsFactory.CreateDurationMetric("sync-logins.duration");

        var set = await adapter.GetLogins();
        await repository.AddSyncSet(set);
        
        logger.LogInformation("SyncLoginsHandler completed");
        return Unit.Value;
    }
}