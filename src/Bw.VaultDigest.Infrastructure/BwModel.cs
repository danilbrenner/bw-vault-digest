namespace Bw.VaultDigest.Infrastructure;

public class ApiKeys
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
}

public class StatusInfo
{
    public string? UserEmail { get; set; }
    public required string Status { get; init; }
    public DateTime? LastSync { get; init; }
    public string? ServerUrl { get; init; }
}

public enum CipherType
{
    Login = 1,
    SecureNote = 2,
    Card = 3,
    Identity = 4
}

public class PasswordHistoryItem
{
    public required DateTime LastUsedDate { get; init; }
    public required string Password { get; init; }
}

public class LoginUri
{
    public string? Match { get; init; }
    public required string Uri { get; init; }
}

public class LoginContent
{
    // public required List<????????> Fido2Credentials { get; init; } = [];
    public List<LoginUri>? Uris { get; init; } = [];
    public required string Username { get; init; }
    public required string Password { get; init; }
    public string? Totp { get; init; }
    public DateTime? PasswordRevisionDate { get; init; }
}

public class CardContent
{
    public required string CardholderName { get; init; }
    public required string Brand { get; init; }
    public required string Number { get; init; }
    public required string ExpMonth { get; init; }
    public required string ExpYear { get; init; }
    public required string Code { get; init; }
}

public class SecureNoteContent
{
    public required int Type { get; init; }
}

public class Item
{
    public required List<PasswordHistoryItem> PasswordHistory { get; init; } = [];
    public required DateTime RevisionDate { get; init; }
    public required DateTime CreationDate { get; init; }
    public DateTime? DeletedDate { get; init; }
    public required string Object { get; init; }
    public required Guid Id { get; init; }
    public Guid? OrganizationId { get; init; }
    public Guid? FolderId { get; init; }
    public required CipherType Type { get; init; }
    public required int Reprompt { get; init; }
    public required string Name { get; init; }
    public string? Notes { get; init; }
    public required bool Favorite { get; init; }
    public required List<Guid> CollectionIds { get; init; } = [];
    public LoginContent? Login { get; init; }
    public SecureNoteContent? SecureNote { get; init; }
    public CardContent? Card { get; init; }
}