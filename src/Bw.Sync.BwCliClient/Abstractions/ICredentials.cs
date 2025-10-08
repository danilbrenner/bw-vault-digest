using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.Abstractions;

public interface ICredentials : IDisposable
{
    ReadOnlyMemory<byte> Password { get; }
    ApiKeys ApiKeys { get; }
}

