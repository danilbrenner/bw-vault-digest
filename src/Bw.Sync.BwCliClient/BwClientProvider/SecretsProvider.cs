using System.Security;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.BwClientProvider;

public class SecretsProvider : ISecretsProvider, IDisposable
{
    private byte[]? _password;
    private byte[]? _clientId;
    private byte[]? _clientSecret;

    public Task<ApiKeys?> GetApiKeys()
    {
        if (_clientId == null || _clientSecret == null)
            return Task.FromResult<ApiKeys?>(null);
        var clientId = System.Text.Encoding.UTF8.GetString(_clientId);
        var clientSecret = System.Text.Encoding.UTF8.GetString(_clientSecret);
        return Task.FromResult<ApiKeys?>(new ApiKeys(clientId, clientSecret));
    }

    public Task<string?> GetPassword()
    {
        if (_password == null)
            return Task.FromResult<string?>(null);
        var password = System.Text.Encoding.UTF8.GetString(_password);
        return Task.FromResult<string?>(password);
    }

    public Task SetSecrets(string password, string clientId, string clientSecret)
    {
        _password = string.IsNullOrEmpty(password) ? null : System.Text.Encoding.UTF8.GetBytes(password);
        _clientId = string.IsNullOrEmpty(clientId) ? null : System.Text.Encoding.UTF8.GetBytes(clientId);
        _clientSecret = string.IsNullOrEmpty(clientSecret) ? null : System.Text.Encoding.UTF8.GetBytes(clientSecret);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_password != null) Array.Clear(_password, 0, _password.Length);
        if (_clientId != null) Array.Clear(_clientId, 0, _clientId.Length);
        if (_clientSecret != null) Array.Clear(_clientSecret, 0, _clientSecret.Length);
        _password = null;
        _clientId = null;
        _clientSecret = null;
    }
}