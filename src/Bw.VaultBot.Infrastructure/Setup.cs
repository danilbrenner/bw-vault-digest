using Bw.VaultBot.Infrastructure.Abstractions;
using Bw.VaultBot.Infrastructure.BwClientProvider;
using Bw.VaultBot.Infrastructure.Options;
using Bw.VaultBot.Infrastructure.EmailNotifierClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.VaultBot.Infrastructure;

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