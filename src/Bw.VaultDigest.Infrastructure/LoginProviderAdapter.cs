using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure;

// using MLogin = Bw.VaultDigest.Model.Login;

public interface ILoginProviderAdapter
{
    Task<IEnumerable<Login>> GetLogins();
}

public class LoginProviderAdapter(IBwClient client) : ILoginProviderAdapter
{
    public async Task<IEnumerable<Login>> GetLogins()
    {
        var itms = await client.GetItems();
        return itms.ToLogins(DateTime.Today);
    }
}