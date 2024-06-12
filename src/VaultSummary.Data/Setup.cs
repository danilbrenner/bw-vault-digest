using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VaultSummary.Data;

public static class Setup
{
    public static IServiceCollection AddData(this IServiceCollection svc, string connectionString)
    {
        return svc.AddDbContext<VaultSummaryContext>(
            opt => opt.UseSqlServer(connectionString));
    }

    public static void ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var cfg = scope.ServiceProvider.GetService<VaultSummaryContext>();
        cfg?.Database.Migrate();
    }
}