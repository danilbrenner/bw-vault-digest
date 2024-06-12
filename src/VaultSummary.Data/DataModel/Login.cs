namespace VaultSummary.Data.DataModel;

public class Login
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required ICollection<Password> Passwords { get; set; }
}