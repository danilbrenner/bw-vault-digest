using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace VaultSummary.Infrastructure;

public class SecretManagerOptions
{
    public required string VaultUrl { get; init; }
}

public interface ISecretManagerClient
{
    Task<ApiKeys?> GetApiKeys();
    Task<string?> GetPassword();
}

public class AzureSecretManagerClient : ISecretManagerClient
{
    private readonly SecretClient _client;

    public AzureSecretManagerClient(IOptions<SecretManagerOptions> options)
    {
        var cred = new DefaultAzureCredential();
        _client = new SecretClient(new Uri(options.Value.VaultUrl), cred);
    }

    public async Task<ApiKeys?> GetApiKeys()
    {
        var clientId = await _client.GetSecretAsync("bw-clientid");
        var clientSecret = await _client.GetSecretAsync("bw-clientsecret");
        if (clientId.HasValue && clientSecret.HasValue)
            return new ApiKeys { ClientId = clientId.Value.Value, ClientSecret = clientSecret.Value.Value };
        return null;
    }

    public async Task<string?> GetPassword()
    {
        var response = await _client.GetSecretAsync("bw-password");
        return response.HasValue ? response.Value.Value : null;
    }
}