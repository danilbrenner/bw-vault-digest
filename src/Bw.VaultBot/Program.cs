using Bw.VaultBot.Infrastructure;
using Bw.VaultBot;
using Bw.VaultBot.Options;
using Bw.VaultBot.HostedServices;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
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
                    .AddInfrastructure(cfg)
                    .Configure<ScheduleOptions>(cfg.GetSection("Schedule"))
                    .AddHostedService<SyncService>();
            });

    builder.Logging.ClearProviders().AddSerilog();

    var app = builder.Build();

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