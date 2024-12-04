using Bw.VaultDigest.Infrastructure.BwClientProvider;

namespace Bw.VaultDigest.Infrastructure.Abstractions;

public interface ISecretManagerClient
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
}