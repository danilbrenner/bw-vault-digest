using Bw.VaultBot.BwCliClient;
using Bw.VaultBot.Options;
using Bw.VaultBot.Filters;
using Bw.VaultBot.Data;
using Bw.VaultBot.Jobs;
using Serilog;
using Hangfire;
using Hangfire.Dashboard;
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
    var connectionString = configuration.GetConnectionString("HangfireConnection");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.ConfigureServices((cfg, svc) =>
    {
        svc
            .AddAuthorizationFilter(connectionString!)
            .AddInfrastructure(cfg.Configuration)
            .Configure<ScheduleOptions>(cfg.Configuration.GetSection("Schedule"))
            .AddHangfire(x => x
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }))
            .AddHangfireServer()
            .AddHangfireJobs();
    });
    builder.Logging.ClearProviders();
    builder.Logging.AddSerilog();

    var app = builder.Build();

    app.Lifetime.ApplicationStopping.Register(() => cancellationTokenSource.Cancel());

    app
        .MigrateDashboardUsers(connectionString!)
        .UseHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "Vault Sync",
            DarkModeEnabled = true,
            AsyncAuthorization =
            [
                app.Services.GetRequiredService<IDashboardAsyncAuthorizationFilter>()
            ]
        })
        .UseHangfireJobs();

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