using Bw.Sync.BwCliClient.BwClientProvider;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface IBwClient
{
    public Task<string> GetUserEmail();
    public Task<IReadOnlyList<Item>> GetItems();
}