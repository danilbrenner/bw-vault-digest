using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins(ICredentials credentials);
}