using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.BwClientProvider;

public class SecurityCredentials(string password, ApiKeys apiKeys) : ICredentials
{
    public string Password => password;
    public ApiKeys ApiKeys => apiKeys;
}