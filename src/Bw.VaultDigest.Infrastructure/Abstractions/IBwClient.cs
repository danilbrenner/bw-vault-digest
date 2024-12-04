using Bw.VaultDigest.Infrastructure.BwClientProvider;

namespace Bw.VaultDigest.Infrastructure.Abstractions;

public interface IBwClient
{
    public Task<string> GetUserEmail();
    public Task<IReadOnlyList<Item>> GetItems();
}