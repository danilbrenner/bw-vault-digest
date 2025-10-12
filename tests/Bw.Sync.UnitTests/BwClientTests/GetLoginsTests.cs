using AutoFixture.Xunit2;
using Bw.Sync.BwCliClient;
using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.BwClientProvider;
using Bw.Sync.BwCliClient.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq.Protected;

namespace Bw.Sync.UnitTests.BwClientTests;

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

    public GetLoginsTests()
    {
        _bwClientMock = new Mock<BwClient>(
            _dateTimeProviderMock.Object,
            NullLogger<BwClient>.Instance);
    }

    private static EnvVariables EmptyEnvVars => new Dictionary<string, string>();

    private void SetupLogin(string clientId, string clientSecret)
    {
        _bwClientMock.Protected().As<ITest>()
            .Setup(c =>
                c.Request<string>(
                    EmptyEnvVars
                        .FAdd("BW_CLIENTID", clientId)
                        .FAdd("BW_CLIENTSECRET", clientSecret),
                    "login",
                    "--apikey"))
            .ReturnsAsync("");
    }

    private void SetupUnlock(string sessionToken)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c =>
                c.Request<string>(
                    It.Is<EnvVariables>(d => d.ContainsKey("BW_PASSWORD")),
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

    private void VerifyUnlockCalled(Times times)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(
                c =>
                    c.Request<string>(
                        It.Is<EnvVariables>(d => d.ContainsKey("BW_PASSWORD")),
                        "unlock",
                        "--passwordenv", "BW_PASSWORD", "--raw"),
                times);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenUnauthorized(string password, string sessionToken, ApiKeys keys, List<Item> items,
        DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "unauthenticated" })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });

        SetupLogin(keys.ClientId, keys.ClientSecret);
        SetupUnlock(sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(keys);
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginCalled(keys.ClientId, keys.ClientSecret, Times.Once());
        VerifyUnlockCalled(Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenLocked(string password, string sessionToken, List<Item> items, DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginNotCalled();
        VerifyUnlockCalled(Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenResultFailed(string password, string sessionToken, DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(sessionToken);
        SetupGetLogins(sessionToken, null);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        Func<Task> act = () => _bwClientMock.Object.GetItems(credentialsMock.Object);

        await act
            .ShouldThrowAsync<Exception>("Failed to get logins");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
    }

    [Fact]
    public async Task GetLogins_WhenUnknownStatusReceived()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = _fixture.Create<string>() });

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(_fixture.Create<string>());
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        Func<Task> act = () => _bwClientMock.Object.GetItems(credentialsMock.Object);

        await act
            .ShouldThrowAsync<Exception>("Unknown status received");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(Times.Never());
        VerifyGetLoginsCalled(It.IsAny<string>(), Times.Never());
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_ShouldSyncWhenOutdated(string password, string sessionToken, List<Item> items,
        DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status"))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now.AddDays(-2) })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginNotCalled();
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(c => c.Request<string>(EmptyEnvVars, "sync"), Times.Once());
        VerifyUnlockCalled(Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }
}