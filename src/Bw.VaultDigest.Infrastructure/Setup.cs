using Bw.VaultDigest.Infrastructure.Abstractions;
using Bw.VaultDigest.Infrastructure.BwClientProvider;
using Bw.VaultDigest.Infrastructure.EmailNotifierClient;
using Bw.VaultDigest.Infrastructure.Options;
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
                .Configure<EmailNotifierOptions>(config.GetSection("EmailDigest:EmailNotifier"))
                .AddTransient<ISecretManagerClient, AzureSecretManagerClient>()
                .AddTransient<IBwClient, BwClient>()
                .AddTransient<ILoginProviderAdapter, LoginProviderAdapter>()
                .AddTransient<IEmailNotifier, EmailNotifier>()
                .AddTransient<DateTimeProvider>();
    }
}