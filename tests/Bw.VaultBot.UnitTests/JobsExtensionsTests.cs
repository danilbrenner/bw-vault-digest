
namespace Bw.VaultBot.UnitTests
{
    public static class One
    {
        public static string ToCsvFileName(this DateTime dateTime)
            => $"logins_{dateTime:yyyyMMddHHmmss}.csv";
    }
    
    public class JobsExtensionsTests
    {
        [Fact]
        public void ToCsvFileName_ReturnsExpectedFormat()
        {
            var date = new DateTime(2024, 6, 1, 14, 30, 45);
            var result = date.ToCsvFileName();
            result.ShouldBe("logins_20240601143045.csv");
        }

        [Fact]
        public void ToCsvFileName_ZeroPaddedValues()
        {
            var date = new DateTime(2024, 1, 2, 3, 4, 5);
            var result = date.ToCsvFileName();
            result.ShouldBe("logins_20240102030405.csv");
        }
    }
}