using Bw.VaultBot.Infrastructure.BwClientProvider;

namespace Bw.VaultBot.Infrastructure.Abstractions;

public interface ISecretManagerClient
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
}