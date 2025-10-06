using Bw.Sync.BwCliClient.Abstractions;
using Bw.Sync.BwCliClient.BwClientProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bw.Sync.BwCliClient;

public static class Setup
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection svc, IConfiguration config)
    {
        return
            svc
                .AddTransient<IBwClient, BwClient>()
                .AddTransient<ILoginProviderAdapter, LoginProviderAdapter>()
                .AddTransient<DateTimeProvider>();
    }
}