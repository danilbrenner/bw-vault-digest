using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;
using Bw.VaultBot.Model;

namespace Bw.Sync.BwCliClient.BwClientProvider;

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<LoginsSet> GetLogins(ICredentials credentials)
    {
        var userEmail = await client.GetUserEmail(credentials);
        var logins = await client.GetItems(credentials);
        return new LoginsSet(Guid.NewGuid(), userEmail, DateTime.Now, logins.ToLogins());
    }
}