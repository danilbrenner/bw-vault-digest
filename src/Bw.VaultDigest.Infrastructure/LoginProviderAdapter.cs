using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure;

public interface ILoginProviderAdapter
{
    Task<IReadOnlyList<Login>> GetLogins();
}

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<IReadOnlyList<Login>> GetLogins()
    {
        var logins = await client.GetItems();
        return logins.ToLogins(DateTime.Today);
    }
}