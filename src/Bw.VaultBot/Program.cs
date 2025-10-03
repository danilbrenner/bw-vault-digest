using Bw.VaultBot.BwCliClient;
using Bw.VaultBot.Options;
using Bw.VaultBot.HostedServices;
using Serilog;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

try
{
    var cancellationTokenSource = new CancellationTokenSource();

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureServices((cfg, svc) =>
    {
        svc
            .AddInfrastructure(cfg.Configuration)
            .Configure<ScheduleOptions>(cfg.Configuration.GetSection("Schedule"))
            .AddHangfire(x => x
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(cfg.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }));
        svc.AddHangfireServer();
        svc.AddScoped<SyncService>();
    });
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    var app = builder.Build();

    app.Lifetime.ApplicationStopping.Register(() => cancellationTokenSource.Cancel());

    app.UseHangfireDashboard();

    RecurringJob.AddOrUpdate<SyncService>("sync-service-execute", job => job.Execute(), Cron.Daily);

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