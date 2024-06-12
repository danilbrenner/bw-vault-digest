using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using VaultSummary.App;
using VaultSummary.Data;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

try
{
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
                    .AddVaultSummaryHealth()
                    .AddSerilog();
            });

    builder
        .Host
        .UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });

    var app = builder.Build();

    app.ApplyMigrations();
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = HealthCheckExtensions.WriteResponse
    });

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}