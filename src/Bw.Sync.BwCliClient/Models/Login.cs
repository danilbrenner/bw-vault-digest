
namespace Bw.VaultBot.Model;

public enum Strength
{
    VeryWeak = 0,
    Weak = 1,
    Moderate = 2,
    Strong = 3,
    VeryStrong = 4
}

public record Login(Guid Id, string Name, string? Username, DateTime ChangedAt, Strength Strength);

public record LoginsSet(Guid Id, string UserEmail, DateTime Timestamp, IReadOnlyList<Login> Logins);
