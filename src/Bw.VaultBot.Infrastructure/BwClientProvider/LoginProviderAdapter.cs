using Bw.VaultBot.Infrastructure.Abstractions;
using Bw.VaultBot.Model;

namespace Bw.VaultBot.Infrastructure.BwClientProvider;

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<LoginsSet> GetLogins()
    {
        var userEmail = await client.GetUserEmail();
        var logins = await client.GetItems();
        return new LoginsSet(Guid.NewGuid(), userEmail, DateTime.Now, logins.ToLogins(DateTime.Today));
    }
}