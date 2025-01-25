using Bw.VaultDigest.Infrastructure.EmailNotifierClient;
using Bw.VaultDigest.Model;
using ScottPlot;

namespace Bw.VaultDigest.UnitTests.Services;

public class DigestServiceTests
{
    private readonly Fixture _fixture = new();

    private IEnumerable<Login> CreateLogins(int count, Strength strength = Strength.Moderate, Age age = Age.Moderate)
    {
        return _fixture
            .Build<Login>()
            .With(p => p.Strength, strength)
            .With(p => p.Age, age)
            .CreateMany(count);
    }

    [Fact]
    public void ToStrengthSlices_Spec()
    {
        var logins =
            CreateLogins(5, strength: Strength.VeryWeak)
                .Union(CreateLogins(7, strength: Strength.Weak))
                .Union(CreateLogins(9, strength: Strength.Moderate))
                .Union(CreateLogins(12, strength: Strength.Strong))
                .Union(CreateLogins(15, strength: Strength.VeryStrong));

        var result =
            logins.ToList().ToStrengthSlices();

        result.Should().HaveCount(5);
        result.Should().Contain(s => s.FillColor == Colors.Red && s.Value == 5);
        result.Should().Contain(s => s.FillColor == Colors.Orange && s.Value == 7);
        result.Should().Contain(s => s.FillColor == Colors.Yellow && s.Value == 9);
        result.Should().Contain(s => s.FillColor == Colors.LightGreen && s.Value == 12);
        result.Should().Contain(s => s.FillColor == Colors.Green && s.Value == 15);
    }

    [Fact]
    public void ToAgesSlices_Spec()
    {
        var logins =
            CreateLogins(5, age: Age.Ancient)
                .Union(CreateLogins(7, age: Age.Old))
                .Union(CreateLogins(9, age: Age.Moderate))
                .Union(CreateLogins(12, age: Age.Recent))
                .Union(CreateLogins(15, age: Age.New));

        var result =
            logins.ToList().ToAgeSlices();

        result.Should().HaveCount(5);
        result.Should().Contain(s => s.FillColor == Colors.Red && s.Value == 5);
        result.Should().Contain(s => s.FillColor == Colors.Orange && s.Value == 7);
        result.Should().Contain(s => s.FillColor == Colors.Yellow && s.Value == 9);
        result.Should().Contain(s => s.FillColor == Colors.LightGreen && s.Value == 12);
        result.Should().Contain(s => s.FillColor == Colors.Green && s.Value == 15);
    }
}