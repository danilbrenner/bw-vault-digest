using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Data.Abstractions;

public interface ISyncSetRepository
{
    Task<LoginsSet?> GetLatestSyncSet();
    Task AddSyncSet(LoginsSet syncSet);
}