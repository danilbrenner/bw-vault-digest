using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure.BwClientProvider;

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<LoginsSet> GetLogins()
    {
        var userEmail = await client.GetUserEmail();
        var logins = await client.GetItems();
        return new LoginsSet(Guid.NewGuid(), userEmail, DateTime.Now, logins.ToLogins(DateTime.Today));
    }
}