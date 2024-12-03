using Bw.VaultDigest.Data;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Web;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Web.HostedServices;
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
        Host.CreateApplicationBuilder(args)
            .ConfigureServices((cfg, svc) =>
            {
                svc
                    .AddMetrics()
                    .AddTelemetry()
                    .AddInfrastructure(cfg)
                    .Configure<ScheduleOptions>(cfg.GetSection("Schedule"))
                    .AddHostedService<SyncService>()
                    .AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly))
                    .AddData(cfg);
            });

    builder.Logging.ClearProviders().AddSerilog();

    var app = builder.Build();

    app
        .Services
        .ApplyMigrations();
    
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