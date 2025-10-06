using System;
using System.Threading.Tasks;
using Bw.Sync.BwCliClient.BwClientProvider;
using Bw.Sync.BwCliClient.Models;
using Shouldly;
using Xunit;

namespace Bw.VaultBot.UnitTests.BwClientProvider
{
    public class SecretsProviderTests
    {
        [Fact]
        public async Task SetSecrets_And_GetPassword_Works()
        {
            var provider = new SecretsProvider();
            await provider.SetSecrets("mypassword", "id", "secret");
            var password = await provider.GetPassword();
            password.ShouldBe("mypassword");
        }

        [Fact]
        public async Task SetSecrets_And_GetApiKeys_Works()
        {
            var provider = new SecretsProvider();
            await provider.SetSecrets("mypassword", "id", "secret");
            var apiKeys = await provider.GetApiKeys();
            apiKeys.ShouldNotBeNull();
            apiKeys.ClientId.ShouldBe("id");
            apiKeys.ClientSecret.ShouldBe("secret");
        }

        [Fact]
        public async Task GetPassword_ReturnsNull_IfNotSet()
        {
            var provider = new SecretsProvider();
            var password = await provider.GetPassword();
            password.ShouldBeNull();
        }

        [Fact]
        public async Task GetApiKeys_ReturnsNull_IfNotSet()
        {
            var provider = new SecretsProvider();
            var apiKeys = await provider.GetApiKeys();
            apiKeys.ShouldBeNull();
        }

        [Fact]
        public async Task SetSecrets_WithEmptyValues_ClearsSecrets()
        {
            var provider = new SecretsProvider();
            await provider.SetSecrets("mypassword", "id", "secret");
            await provider.SetSecrets("", "", "");
            (await provider.GetPassword()).ShouldBeNull();
            (await provider.GetApiKeys()).ShouldBeNull();
        }

        [Fact]
        public async Task Dispose_ClearsSecrets()
        {
            var provider = new SecretsProvider();
            await provider.SetSecrets("mypassword", "id", "secret");
            provider.Dispose();
            (await provider.GetPassword()).ShouldBeNull();
            (await provider.GetApiKeys()).ShouldBeNull();
        }
    }
}

