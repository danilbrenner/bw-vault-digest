using Bw.VaultDigest.Web;
using Bw.VaultDigest.Infrastructure;
using Bw.VaultDigest.Web.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

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
                    .AddVaultDigestHealth()
                    .AddSerilog()
                    .AddInfrastructure(cfg)
                    .AddTransient<DigestService>()
                    .Configure<Schedule>(cfg.GetSection("Schedule"))
                    .AddHostedService<RepeaterService>();
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