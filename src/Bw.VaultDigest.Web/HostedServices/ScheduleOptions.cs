namespace Bw.VaultDigest.Web.HostedServices;

public class ScheduleOptions
{
    public required string SyncLogins { get; init; }
    public required string SendDigest { get; init; }
}