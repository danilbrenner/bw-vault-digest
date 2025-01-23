using Bw.VaultDigest.Application;
using Bw.VaultDigest.Data;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Bot;
using Bw.VaultDigest.Bot.Options;
using Bw.VaultDigest.Telemetry;
using Bw.VaultDigest.Bot.HostedServices;
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
    var cancellationTokenSource = new CancellationTokenSource();

    var builder =
        Host.CreateApplicationBuilder(args)
            .ConfigureServices((cfg, svc) =>
            {
                svc
                    .AddMetrics()
                    .AddTelemetry()
                    .AddInfrastructure(cfg)
                    .Configure<ScheduleOptions>(cfg.GetSection("Schedule"))
                    .AddHostedService<SendDigestService>()
                    .AddHostedService<SyncService>()
                    .AddApplication(cfg, cancellationTokenSource.Token)
                    .AddData(cfg);
            });

    builder.Logging.ClearProviders().AddSerilog();

    var app = builder.Build();

    app
        .Services
        .ApplyMigrations()
        .UseBot(cancellationTokenSource.Token);

    app
        .Services
        .GetRequiredService<IHostApplicationLifetime>()
        .ApplicationStopping.Register(() => cancellationTokenSource.Cancel());

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