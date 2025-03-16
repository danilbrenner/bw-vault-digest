using Bw.VaultBot.Model;

namespace Bw.VaultBot.Data.Abstractions;

public interface ISyncSetRepository
{
    Task<LoginsSet?> GetLatestSyncSet();
    Task AddSyncSet(LoginsSet syncSet);
}