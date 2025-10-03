using Bw.VaultBot.BwCliClient.BwClientProvider;
using Bw.VaultBot.BwCliClient.Models;

namespace Bw.VaultBot.BwCliClient.Abstractions;

public interface ISecretManagerClient
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
}