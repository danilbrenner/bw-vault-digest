using System.Buffers;
using System.Security.Cryptography;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.Models;

namespace Bw.Sync.BwCliClient.BwClientProvider;

public class SecureCredentials : ICredentials
{
    private IMemoryOwner<byte>? _owner;
    private readonly int _length;
    private bool _isDisposed;

    public SecureCredentials(ReadOnlySpan<byte> source, ApiKeys apiKeys)
    {
        ApiKeys = apiKeys;
        _owner = MemoryPool<byte>.Shared.Rent(source.Length);
        source.CopyTo(_owner.Memory.Span);
        _length = source.Length;
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        if (_owner is null) return;
        CryptographicOperations.ZeroMemory(_owner.Memory.Span);
        _owner.Dispose();
        _owner = null;
    }

    public ReadOnlyMemory<byte> Password => !_isDisposed ? _owner!.Memory[.._length] : ReadOnlyMemory<byte>.Empty;
    public ApiKeys ApiKeys { get; }
}