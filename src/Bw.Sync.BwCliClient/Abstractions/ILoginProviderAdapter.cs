using Bw.Sync.BwCliClient.Models;
using Bw.VaultBot.Model;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins(ICredentials credentials);
}