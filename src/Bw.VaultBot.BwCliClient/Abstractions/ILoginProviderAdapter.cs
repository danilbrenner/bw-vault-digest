using Bw.VaultBot.Model;

namespace Bw.VaultBot.BwCliClient.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins();
}