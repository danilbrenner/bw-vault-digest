using Bw.VaultBot.BwCliClient.BwClientProvider;

namespace Bw.VaultBot.BwCliClient.Abstractions;

public interface IBwClient
{
    public Task<string> GetUserEmail();
    public Task<IReadOnlyList<Item>> GetItems();
}