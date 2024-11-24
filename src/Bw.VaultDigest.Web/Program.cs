using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Web;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.HostedServices;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.OpenTelemetry(o =>
    {
        o.ResourceAttributes = new Dictionary<string, object>
        {
            { "service.name", TelemetryConstants.OpenTelemetryServiceName }
        };
    })
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .CreateLogger();

try
{
    var builder =
        WebApplication
            .CreateBuilder(args)
            .ConfigureServices((cfg, svc) =>
            {
                svc
                    .AddVaultDigestHealth()
                    .AddMetrics()
                    .AddTelemetry()
                    .AddInfrastructure(cfg)
                    .Configure<Schedule>(cfg.GetSection("Schedule"))
                    .AddHostedService<SyncService>()
                    .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
            });

    builder.Logging.ClearProviders().AddSerilog();

    builder
        .Host
        .UseSerilog();

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