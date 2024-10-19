using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using VaultSummary.App;
using VaultSummary.Infrastructure;

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
                svc
                    .AddVaultSummaryHealth()
                    .AddSerilog()
                    .AddInfrastructure(cfg);
            });

    builder
        .Host
        .UseSerilog((context, configuration) => { configuration.ReadFrom.Configuration(context.Configuration); });

    var app = builder.Build();

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