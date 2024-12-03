namespace Bw.VaultDigest.Infrastructure.Abstractions;

public interface IEmailTemplateLoader
{
    Task<string> RenderMessage(int loginsCount, string cratedFor, DateTime createdAt);
}