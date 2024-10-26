using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.VaultDigest.Infrastructure;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection svc, IConfiguration config)
    {
        return
            svc
                .Configure<SecretManagerOptions>(config.GetSection("SecretManager"))
                .AddTransient<ISecretManagerClient, AzureSecretManagerClient>()
                .AddTransient<IBwClient, BwClient>()
                .AddTransient<ILoginProviderAdapter, LoginProviderAdapter>();
    }
}