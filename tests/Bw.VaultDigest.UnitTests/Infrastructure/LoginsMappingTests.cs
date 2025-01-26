using Bw.VaultDigest.Infrastructure.BwClientProvider;
using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.UnitTests.Infrastructure;

public class LoginsMappingTests
{
    private static char GetRandomCharacter(string characters)
    {
        return characters[Random.Next(characters.Length)];
    }

    private static string GeneratePassword(
        int length,
        bool withNumbers = true,
        bool withLowercase = false,
        bool withUppercase = false,
        bool withSymbols = false)
    {
        var allChars = string.Empty;
        var starters = string.Empty;

        if (withLowercase)
        {
            const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
            allChars += lowercaseChars;
            starters += GetRandomCharacter(lowercaseChars);
        }

        if (withUppercase)
        {
            const string upperCaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            allChars += upperCaseChars;
            starters += GetRandomCharacter(upperCaseChars);
        }

        if (withNumbers)
        {
            const string digits = "0123456789";
            allChars += digits;
            starters += GetRandomCharacter(digits);
        }

        if (withSymbols)
        {
            const string symbols = "!@#$%^&*";
            allChars += symbols;
            starters += GetRandomCharacter(symbols);
        }

        var passwordChars = 
            Enumerable.Range(0, length - starters.Length)
                .Select(_ => allChars[Random.Next(allChars.Length)])
                .Concat(starters.ToCharArray())
                .OrderBy(_ => Random.Next())
                .ToArray();

        return new string(passwordChars);
    }

    private readonly Fixture _fixture = new();

    private static readonly Random Random = new();

    private Item GenLoginItem(
        Func<string> passwordGenerator,
        Func<DateTime> revisionDateGenerator)
    {
        return _fixture
            .Build<Item>()
            .With(l => l.Type, CipherType.Login)
            .With(l => l.Login,
                _fixture
                    .Build<LoginContent>()
                    .With(l => l.Password, passwordGenerator())
                    .With(l => l.PasswordRevisionDate, revisionDateGenerator())
                    .Create())
            .Without(l => l.SecureNote)
            .Without(l => l.Card)
            .Create();
    }

    [Fact]
    public void LoginMapping_ShouldMapLoginsOnly()
    {
        var loginItems =
            _fixture.Build<Item>()
                .With(l => l.Type, CipherType.Login)
                .Without(l => l.SecureNote)
                .Without(l => l.Card)
                .CreateMany()
                .ToArray();

        var items =
            loginItems
                .Union(_fixture.Build<Item>()
                    .With(l => l.Type, CipherType.SecureNote)
                    .Without(l => l.Login)
                    .Without(l => l.Card)
                    .CreateMany())
                .Union(_fixture.Build<Item>()
                    .With(l => l.Type, CipherType.Identity)
                    .Without(l => l.Login)
                    .Without(l => l.Card)
                    .Without(l => l.SecureNote)
                    .CreateMany())
                .Union(_fixture.Build<Item>()
                    .With(l => l.Type, CipherType.Card)
                    .Without(l => l.Login)
                    .Without(l => l.SecureNote)
                    .CreateMany())
                .ToList();

        var logins = items.ToLogins(DateTime.Now);

        logins.Count.ShouldBe(loginItems.Length);
        logins.ShouldContain(l => loginItems.Any(li => li.Id == l.Id));
        logins.ShouldNotContain(l => loginItems.All(li => li.Id != l.Id));
    }

    private class StrengthCalculationData : TheoryData<Strength, string>
    {
        public StrengthCalculationData()
        {
            // Numbers only
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 12 => Strength.VeryWeak,
                    _ => Strength.Weak
                }, GeneratePassword(i));
            }

            // Lowercase only
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 9 => Strength.VeryWeak,
                    < 14 => Strength.Weak,
                    < 18 => Strength.Moderate,
                    _ => Strength.Strong
                }, GeneratePassword(i, withNumbers: false, withLowercase: true));
            }

            // Uppercase only
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 9 => Strength.VeryWeak,
                    < 14 => Strength.Weak,
                    < 18 => Strength.Moderate,
                    _ => Strength.Strong
                }, GeneratePassword(i, withNumbers: false, withUppercase: true));
            }

            // Upper & Lowercase
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 7 => Strength.VeryWeak,
                    < 12 => Strength.Weak,
                    < 15 => Strength.Moderate,
                    < 18 => Strength.Strong,
                    _ => Strength.VeryStrong
                }, GeneratePassword(i, withNumbers: false, withUppercase: true, withLowercase: true));
            }

            // Numbers & Upper & Lowercase
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 7 => Strength.VeryWeak,
                    < 11 => Strength.Weak,
                    < 14 => Strength.Moderate,
                    < 17 => Strength.Strong,
                    _ => Strength.VeryStrong
                }, GeneratePassword(i, withNumbers: true, withUppercase: true, withLowercase: true));
            }

            // Numbers & Upper & Lowercase & Symbols
            for (var i = 4; i < 20; i++)
            {
                Add(i switch
                {
                    < 7 => Strength.VeryWeak,
                    < 11 => Strength.Weak,
                    < 13 => Strength.Moderate,
                    < 16 => Strength.Strong,
                    _ => Strength.VeryStrong
                }, GeneratePassword(i, withNumbers: true, withUppercase: true, withLowercase: true, withSymbols: true));
            }
        }
    }

    [Theory]
    [ClassData(typeof(StrengthCalculationData))]
    public void LoginMapping_StrengthCalculation(Strength expectedStrength, string password)
    {
        var item = _fixture
            .Build<Item>()
            .With(l => l.Type, CipherType.Login)
            .Without(l => l.SecureNote)
            .Without(l => l.Card)
            .With(l => l.Login,
                _fixture.Build<LoginContent>()
                    .With(l => l.Password, password)
                    .Create())
            .Create();

        IReadOnlyList<Item> items = [item];

        var logins = items.ToLogins(_fixture.Create<DateTime>());

        logins.Count.ShouldBe(1);
        logins[0].Id.ShouldBe(item.Id);
        logins[0].Name.ShouldBe(item.Name);
        logins[0].Strength.ShouldBe(expectedStrength);
    }

    private class AgeCalculationData : TheoryData<Age, DateTime, DateTime>
    {
        public AgeCalculationData()
        {
            var now = new Fixture().Create<DateTime>();
            Add(Age.New, now.AddMonths(-1), now);
            Add(Age.Recent, now.AddMonths(-1).AddDays(-1), now);
            Add(Age.Recent, now.AddMonths(-3), now);
            Add(Age.Moderate, now.AddMonths(-3).AddDays(-1), now);
            Add(Age.Moderate, now.AddMonths(-6), now);
            Add(Age.Old, now.AddMonths(-6).AddDays(-1), now);
            Add(Age.Old, now.AddMonths(-12), now);
            Add(Age.Ancient, now.AddMonths(-12).AddDays(-1), now);
        }
    }

    [Theory]
    [ClassData(typeof(AgeCalculationData))]
    public void LoginMapping_AgeCalculation(Age expectedAge, DateTime date, DateTime now)
    {
        var item = _fixture
            .Build<Item>()
            .With(l => l.Type, CipherType.Login)
            .Without(l => l.SecureNote)
            .Without(l => l.Card)
            .With(l => l.Login,
                _fixture.Build<LoginContent>()
                    .With(l => l.PasswordRevisionDate, date)
                    .Create())
            .Create();

        IReadOnlyList<Item> items = [item];

        var logins = items.ToLogins(now);

        logins.Count.ShouldBe(1);
        logins[0].Id.ShouldBe(item.Id);
        logins[0].Name.ShouldBe(item.Name);
        logins[0].Age.ShouldBe(expectedAge);
    }
}