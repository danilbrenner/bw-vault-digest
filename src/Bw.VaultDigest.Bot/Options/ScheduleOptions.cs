namespace Bw.VaultDigest.Bot.Options;

public class ScheduleOptions
{
    public required string SyncLogins { get; init; }
    public required string SendDigest { get; init; }
}