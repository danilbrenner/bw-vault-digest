using Bw.VaultBot.Model;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins();
}