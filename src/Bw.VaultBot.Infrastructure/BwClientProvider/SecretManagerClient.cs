using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Bw.VaultBot.Infrastructure.Abstractions;
using Bw.VaultBot.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Bw.VaultBot.Infrastructure.BwClientProvider;

public class AzureSecretManagerClient : ISecretManagerClient
{
    private readonly IOptions<SecretManagerOptions> _options;
    
    public AzureSecretManagerClient(IOptions<SecretManagerOptions> options)
    {
        _options = options;
    }

    private SecretClient GetClient()
    {
        var cred = new DefaultAzureCredential();
        return new SecretClient(new Uri(_options.Value.VaultUrl), cred);

    }
    
    public async Task<ApiKeys?> GetApiKeys()
    {
        var client = GetClient();
        var clientId = await client.GetSecretAsync("bw-clientid");
        var clientSecret = await client.GetSecretAsync("bw-clientsecret");
        if (clientId.HasValue && clientSecret.HasValue)
            return new ApiKeys { ClientId = clientId.Value.Value, ClientSecret = clientSecret.Value.Value };
        return null;
    }

    public async Task<string?> GetPassword()
    {
        var response = await GetClient().GetSecretAsync("bw-password");
        return response.HasValue ? response.Value.Value : null;
    }
}