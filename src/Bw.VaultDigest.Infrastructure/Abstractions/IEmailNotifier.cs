namespace Bw.VaultDigest.Infrastructure.Abstractions;

public interface IEmailNotifier
{
    Task SendEmail(string body, IReadOnlyList<(string, byte[])> images);
}