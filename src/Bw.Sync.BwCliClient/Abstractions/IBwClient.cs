using Bw.Sync.BwCliClient.BwClientProvider;
using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface IBwClient
{
    public Task<string> GetUserEmail(ICredentials credentials);
    public Task<IReadOnlyList<Item>> GetItems(ICredentials credentials);
}