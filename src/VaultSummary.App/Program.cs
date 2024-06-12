using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using VaultSummary.App;
using VaultSummary.Data;

var builder =
    WebApplication
        .CreateBuilder(args)
        .ConfigureServices((cfg, svc) =>
        {
            var connectionString = cfg.GetConnectionString("SqlData");
            if (connectionString is null)
                throw new ArgumentException("SqlData connection string not set");

            svc
                .AddData(connectionString)
                .AddVaultSummaryHealth();
        });

var app = builder.Build();

app.ApplyMigrations();
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckExtensions.WriteResponse
});

app.Run();