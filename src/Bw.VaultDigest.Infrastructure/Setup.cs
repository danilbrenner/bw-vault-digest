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
                .Configure<EmailNotifierOptions>(config.GetSection("EmailNotifierOptions"))
                .Configure<EmailTemplates>(config.GetSection("EmailTemplates"))
                .AddTransient<ISecretManagerClient, AzureSecretManagerClient>()
                .AddTransient<IBwClient, BwClient>()
                .AddTransient<ILoginProviderAdapter, LoginProviderAdapter>()
                .AddTransient<IEmailNotifier, EmailNotifier>()
                .AddTransient<IEmailTemplateLoader, EmailTemplateLoader>();
    }
}