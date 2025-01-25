using AutoFixture.Xunit2;
using Bw.VaultDigest.Common;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Infrastructure.BwClientProvider;
using Microsoft.Extensions.Logging.Abstractions;
using Moq.Protected;

namespace Bw.VaultDigest.UnitTests.Infrastructure.BwClientTests;

using EnvVariables = IDictionary<string, string>;

public interface ITest
{
    public Task<T> Request<T>(EnvVariables envVariables, string command, params string[] options);
}

public class GetLoginsTests
{
    private static readonly EnvVariables AnyEnvVars = It.IsAny<Dictionary<string, string>>();
    private readonly Mock<BwClient> _bwClientMock;
    private readonly Mock<DateTimeProvider> _dateTimeProviderMock = new();

    private readonly Fixture _fixture = new();
    private readonly Mock<ISecretManagerClient> _secretManagerClientMock = new();

    public GetLoginsTests()
    {
        _bwClientMock = new Mock<BwClient>(
            _secretManagerClientMock.Object,
            _dateTimeProviderMock.Object,
            NullLogger<BwClient>.Instance);
    }

    private static EnvVariables EmptyEnvVars => new Dictionary<string, string>();

    private void SetupLogin(string clientId, string clientSecret)
    {
        _bwClientMock.Protected().As<ITest>()
            .Setup(
                c =>
                    c.Request<string>(
                        EmptyEnvVars
                            .FAdd("BW_CLIENTID", clientId)
                            .FAdd("BW_CLIENTSECRET", clientSecret),
                        "login",
                        "--apikey"))
            .ReturnsAsync("");
    }

    private void SetupUnlock(string password, string sessionToken)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c =>
                c.Request<string>(
                    EmptyEnvVars.FAdd("BW_PASSWORD", password),
                    "unlock",
                    "--passwordenv", "BW_PASSWORD", "--raw"))
            .ReturnsAsync(sessionToken);
    }

    private void SetupGetLogins(string sessionToken, IReadOnlyList<Item>? items)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c =>
                c.Request<IReadOnlyList<Item>?>(
                    EmptyEnvVars.FAdd("BW_SESSION", sessionToken),
                    "list",
                    "items"))
            .ReturnsAsync(items);
    }

    private void VerifyGetLoginsCalled(string sessionToken, Times times)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(c =>
                    c.Request<IReadOnlyList<Item>>(
                        EmptyEnvVars.FAdd("BW_SESSION", sessionToken),
                        "list",
                        "items"),
                times);
    }

    private void VerifyLoginCalled(string clientId, string clientSecret, Times times)
    {
        _bwClientMock.Protected().As<ITest>()
            .Verify(
                c =>
                    c.Request<string>(
                        EmptyEnvVars
                            .FAdd("BW_CLIENTID", clientId)
                            .FAdd("BW_CLIENTSECRET", clientSecret),
                        "login",
                        "--apikey"),
                times);
    }

    private void VerifyLoginNotCalled()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(
                c => c.Request<string>(AnyEnvVars, "login", "--apikey"),
                Times.Never());
    }

    private void VerifyUnlockCalled(string password, Times times)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(
                c =>
                    c.Request<string>(
                        EmptyEnvVars.FAdd("BW_PASSWORD", password),
                        "unlock",
                        "--passwordenv", "BW_PASSWORD", "--raw"),
                times);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenUnauthorized(string password, string sessionToken, ApiKeys keys, List<Item> items,
        DateTime now)
    {
        _secretManagerClientMock.Setup(sm => sm.GetApiKeys()).ReturnsAsync(keys);
        _secretManagerClientMock.Setup(sm => sm.GetPassword()).ReturnsAsync(password);
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "unauthenticated" })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });

        SetupLogin(keys.ClientId, keys.ClientSecret);
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var logins = await _bwClientMock.Object.GetItems();

        VerifyLoginCalled(keys.ClientId, keys.ClientSecret, Times.Once());
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.Should().BeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenLocked(string password, string sessionToken, List<Item> items, DateTime now)
    {
        _secretManagerClientMock.Setup(sm => sm.GetPassword()).ReturnsAsync(password);
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var logins = await _bwClientMock.Object.GetItems();

        VerifyLoginNotCalled();
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.Should().BeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenResultFailed(string password, string sessionToken, DateTime now)
    {
        _secretManagerClientMock.Setup(sm => sm.GetPassword()).ReturnsAsync(password);
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, null);

        // var logins = await 
        Func<Task> act = () => _bwClientMock.Object.GetItems();

        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Failed to get logins");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
    }

    [Fact]
    public async Task GetLogins_WhenFailedToGetPassword()
    {
        var now = _fixture.Create<DateTime>();
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });

        Func<Task> act = () => _bwClientMock.Object.GetItems();

        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Could not retrieve master password");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(It.IsAny<string>(), Times.Never());
        VerifyGetLoginsCalled(It.IsAny<string>(), Times.Never());
    }

    [Fact]
    public async Task GetLogins_WhenFailedToGetApiKey()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "unauthenticated" });

        Func<Task> act = () => _bwClientMock.Object.GetItems();

        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Could not retrieve Api Keys to login");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(It.IsAny<string>(), Times.Never());
        VerifyGetLoginsCalled(It.IsAny<string>(), Times.Never());
    }

    [Fact]
    public async Task GetLogins_WhenUnknownStatusReceived()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = _fixture.Create<string>() });

        Func<Task> act = () => _bwClientMock.Object.GetItems();

        await act
            .Should()
            .ThrowAsync<Exception>()
            .WithMessage("Unknown status received");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(It.IsAny<string>(), Times.Never());
        VerifyGetLoginsCalled(It.IsAny<string>(), Times.Never());
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_ShouldSyncWhenOutdated(string password, string sessionToken, List<Item> items,
        DateTime now)
    {
        _secretManagerClientMock.Setup(sm => sm.GetPassword()).ReturnsAsync(password);
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now.AddDays(-2) })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var logins = await _bwClientMock.Object.GetItems();

        VerifyLoginNotCalled();
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(c => c.Request<string>(EmptyEnvVars, "sync"), Times.Once());
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.Should().BeEquivalentTo(items);
    }
}