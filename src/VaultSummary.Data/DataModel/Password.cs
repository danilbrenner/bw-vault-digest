namespace VaultSummary.Data.DataModel;

public class Password
{
    public Guid Id { get; set; }
    public Guid LoginId { get; set; }
    public byte Strength { get; set; }
    public DateTimeOffset LastUsedDate { get; set; }
    public required string Hash { get; set; }
}