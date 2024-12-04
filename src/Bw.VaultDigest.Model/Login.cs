
namespace Bw.VaultDigest.Model;

public enum Age
{
    New = 0,
    Recent = 1,
    Moderate = 2,
    Old = 3,
    Ancient = 4
}

public enum Strength
{
    VeryWeak = 0,
    Weak = 1,
    Moderate = 2,
    Strong = 3,
    VeryStrong = 4
}

public record Login(Guid Id, string Name, Age Age, Strength Strength);

public record LoginsSet(Guid Id, string UserEmail, DateTime Timestamp, IReadOnlyList<Login> Logins);
