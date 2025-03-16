using Bw.VaultBot.Model;

namespace Bw.VaultBot.Infrastructure.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins();
}