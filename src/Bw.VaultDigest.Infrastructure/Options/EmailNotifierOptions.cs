namespace Bw.VaultDigest.Infrastructure.Options;

public class EmailNotifierOptions
{
    public required string From { get; init; }
    public required string To { get; init; }
    public required string SmtpServer { get; init; }
    public required int SmtpPort { get; init; }
    public required string Username { get; init; }
    public required string Password { get; init; }
    public required string DigestTitle { get; init; }
}