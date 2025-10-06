using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ISecretsProvider
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
    Task SetSecrets(string password, string clientId, string clientSecret);
}