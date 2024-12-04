using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure.Abstractions;

public record SendDigestEmailSettings(string To, string Title, string Template);

public interface IEmailNotifier
{
    Task SendDigest(LoginsSet set, SendDigestEmailSettings settings);
    
}