using Bw.VaultBot.BwCliClient.BwClientProvider;

namespace Bw.VaultBot.BwCliClient.Abstractions;

public interface ISecretManagerClient
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
}