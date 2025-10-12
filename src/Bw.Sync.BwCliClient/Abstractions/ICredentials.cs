using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ICredentials
{
    string Password { get; }
    ApiKeys ApiKeys { get; }
}
