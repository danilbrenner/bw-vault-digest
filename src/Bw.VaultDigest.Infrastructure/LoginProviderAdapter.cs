using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure;

public interface ILoginProviderAdapter
{
    Task<LoginsSet> GetLogins();
}

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<LoginsSet> GetLogins()
    {
        var userEmail = await client.GetUserEmail();
        var logins = await client.GetItems();
        return new LoginsSet(userEmail, logins.ToLogins(DateTime.Today));
    }
}