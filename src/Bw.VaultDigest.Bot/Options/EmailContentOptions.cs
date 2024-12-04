namespace Bw.VaultDigest.Bot.Options;

public class EmailContentOptions
{
    public required string To { get; init; }
    public required string Title { get; init; }
    public required string Template { get; init; }
}