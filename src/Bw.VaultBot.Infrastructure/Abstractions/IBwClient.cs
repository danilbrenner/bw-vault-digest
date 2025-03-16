using Bw.VaultBot.Infrastructure.BwClientProvider;

namespace Bw.VaultBot.Infrastructure.Abstractions;

public interface IBwClient
{
    public Task<string> GetUserEmail();
    public Task<IReadOnlyList<Item>> GetItems();
}