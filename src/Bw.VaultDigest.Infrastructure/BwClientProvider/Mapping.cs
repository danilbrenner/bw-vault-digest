using Bw.VaultDigest.Model;

namespace Bw.VaultDigest.Infrastructure.BwClientProvider;

public static class Mapping
{
    private static double MonthDifference(this DateTime lValue, DateTime rValue)
    {
        var monthDiff = (lValue.Year - rValue.Year) * 12 + lValue.Month - rValue.Month;
        var dayDiff = (lValue.Day - rValue.Day) / (double)DateTime.DaysInMonth(rValue.Year, rValue.Month);
        return monthDiff + dayDiff;
    }

    private static Age GetPasswordAge(this Item item, DateTime now)
    {
        return item.Login?.PasswordRevisionDate?.Date.MonthDifference(now.Date) switch
        {
            null => Age.Ancient,
            >= -1 => Age.New,
            >= -3 => Age.Recent,
            >= -6 => Age.Moderate,
            >= -12 => Age.Old,
            _ => Age.Ancient
        };
    }

    private static Strength GetPasswordStrength(this Item item)
    {
        char[] symbols = ['!', '@', '#', '$', '%', '^', '&', '*'];
        var login = item.Login;
        if (login is null)
            return Strength.VeryWeak;
        return (
                login.Password.Length,
                login.Password.Any(char.IsDigit),
                login.Password.Any(char.IsLower),
                login.Password.Any(char.IsUpper),
                login.Password.Any(symbols.Contains)
            )
            switch
            {
                (> 15, /*D*/true, /*L*/true, /*U*/true, /*S*/true) =>
                    Strength.VeryStrong,
                (> 12, /*D*/true, /*L*/true, /*U*/true, /*S*/true) =>
                    Strength.Strong,
                (> 10, /*D*/true, /*L*/true, /*U*/true, /*S*/true) =>
                    Strength.Moderate,
                (> 6, /*D*/true, /*L*/true, /*U*/true, /*S*/true) =>
                    Strength.Weak,
                (> 16, /*D*/true, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.VeryStrong,
                (> 13, /*D*/true, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Strong,
                (> 10, /*D*/true, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Moderate,
                (> 6, /*D*/true, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Weak,
                (> 17, /*D*/false, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.VeryStrong,
                (> 14, /*D*/false, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Strong,
                (> 11, /*D*/false, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Moderate,
                (> 6, /*D*/false, /*L*/true, /*U*/true, /*S*/false) =>
                    Strength.Weak,
                (> 17, /*D*/false, /*L*/false, /*U*/true, /*S*/false) =>
                    Strength.Strong,
                (> 13, /*D*/false, /*L*/false, /*U*/true, /*S*/false) =>
                    Strength.Moderate,
                (> 8, /*D*/false, /*L*/false, /*U*/true, /*S*/false) =>
                    Strength.Weak,
                (> 17, /*D*/false, /*L*/true, /*U*/false, /*S*/false) =>
                    Strength.Strong,
                (> 13, /*D*/false, /*L*/true, /*U*/false, /*S*/false) =>
                    Strength.Moderate,
                (> 8, /*D*/false, /*L*/true, /*U*/false, /*S*/false) =>
                    Strength.Weak,
                (> 11, /*D*/true, /*L*/false, /*U*/false, /*S*/false) =>
                    Strength.Weak,
                _ => Strength.VeryWeak
            };
    }

    public static IReadOnlyList<Login> ToLogins(this IReadOnlyList<Item> logins, DateTime now)
    {
        return
            logins
                .Where(l => l.Type == CipherType.Login)
                .Select(l =>
                    new Login(
                        l.Id,
                        l.Name,
                        l.GetPasswordAge(now),
                        l.GetPasswordStrength()
                    ))
                .ToArray();
    }
}