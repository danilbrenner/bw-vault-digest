using Bw.VaultBot.BwCliClient.Abstractions;
using Bw.VaultBot.BwCliClient.BwClientProvider;
using Bw.VaultBot.BwCliClient.Options;
using Bw.VaultBot.BwCliClient.EmailNotifierClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.VaultBot.BwCliClient;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection svc, IConfiguration config)
    {
        return
            svc
                .Configure<SecretManagerOptions>(config.GetSection("SecretManager"))
                .AddTransient<ISecretManagerClient, AzureSecretManagerClient>()
                .AddTransient<IBwClient, BwClient>()
                .AddTransient<ILoginProviderAdapter, LoginProviderAdapter>()
                .AddTransient<DateTimeProvider>();
    }
}