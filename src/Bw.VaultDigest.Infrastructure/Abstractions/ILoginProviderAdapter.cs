using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure.Abstractions;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins();
}