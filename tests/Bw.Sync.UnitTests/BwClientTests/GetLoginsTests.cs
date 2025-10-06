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
    public Task<T> Request<T>(EnvVariables envVariables, string command, byte[] stdIn, params string[] options);
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
            .Setup(
                c =>
                    c.Request<string>(
                        EmptyEnvVars
                            .FAdd("BW_CLIENTID", clientId)
                            .FAdd("BW_CLIENTSECRET", clientSecret),
                        "login",
                        Array.Empty<byte>(),
                        "--apikey"))
            .ReturnsAsync("");
    }

    private void SetupUnlock(byte[] password, string sessionToken)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c =>
                c.Request<string>(
                    EmptyEnvVars,
                    "unlock",
                    password,
                    "--raw"))
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
                    Array.Empty<byte>(),
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
                        Array.Empty<byte>(),
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
                        Array.Empty<byte>(),
                        "--apikey"),
                times);
    }

    private void VerifyLoginNotCalled()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(
                c => c.Request<string>(AnyEnvVars, "login", Array.Empty<byte>(), "--apikey"),
                Times.Never());
    }

    private void VerifyUnlockCalled(byte[] password, Times times)
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(
                c =>
                    c.Request<string>(
                        EmptyEnvVars,
                        "unlock",
                        password,
                        "--raw"),
                times);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenUnauthorized(byte[] password, string sessionToken, ApiKeys keys, List<Item> items,
        DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status", Array.Empty<byte>()))
            .ReturnsAsync(new StatusInfo { Status = "unauthenticated" })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });

        SetupLogin(keys.ClientId, keys.ClientSecret);
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(keys);
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginCalled(keys.ClientId, keys.ClientSecret, Times.Once());
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenLocked(byte[] password, string sessionToken, List<Item> items, DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status", Array.Empty<byte>()))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginNotCalled();
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_WhenResultFailed(byte[] password, string sessionToken, DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status", Array.Empty<byte>()))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, null);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        Func<Task> act = () => _bwClientMock.Object.GetItems(credentialsMock.Object);

        await act
            .ShouldThrowAsync<Exception>("Failed to get logins");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
    }

    [Fact]
    public async Task GetLogins_WhenUnknownStatusReceived()
    {
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Setup(c => c.Request<StatusInfo>(EmptyEnvVars, "status", Array.Empty<byte>()))
            .ReturnsAsync(new StatusInfo { Status = _fixture.Create<string>() });

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(_fixture.Create<byte[]>());
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        Func<Task> act = () => _bwClientMock.Object.GetItems(credentialsMock.Object);

        await act
            .ShouldThrowAsync<Exception>("Unknown status received");

        VerifyLoginNotCalled();
        VerifyUnlockCalled(It.IsAny<byte[]>(), Times.Never());
        VerifyGetLoginsCalled(It.IsAny<string>(), Times.Never());
    }

    [Theory]
    [AutoData]
    public async Task GetLogins_ShouldSyncWhenOutdated(byte[] password, string sessionToken, List<Item> items,
        DateTime now)
    {
        _dateTimeProviderMock.SetupGet(dp => dp.UtcNow).Returns(now);
        _bwClientMock
            .Protected()
            .As<ITest>()
            .SetupSequence(c => c.Request<StatusInfo>(EmptyEnvVars, "status", Array.Empty<byte>()))
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now.AddDays(-2) })
            .ReturnsAsync(new StatusInfo { Status = "locked", LastSync = now });
        SetupUnlock(password, sessionToken);
        SetupGetLogins(sessionToken, items);

        var credentialsMock = new Mock<ICredentials>();
        credentialsMock.SetupGet(c => c.Password).Returns(password);
        credentialsMock.SetupGet(c => c.ApiKeys).Returns(_fixture.Create<ApiKeys>());
        var logins = await _bwClientMock.Object.GetItems(credentialsMock.Object);

        VerifyLoginNotCalled();
        _bwClientMock
            .Protected()
            .As<ITest>()
            .Verify(c => c.Request<string>(EmptyEnvVars, "sync", Array.Empty<byte>()), Times.Once());
        VerifyUnlockCalled(password, Times.Once());
        VerifyGetLoginsCalled(sessionToken, Times.Once());
        logins.ShouldBeEquivalentTo(items);
    }
}