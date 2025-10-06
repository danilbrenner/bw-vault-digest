using Bw.VaultBot.Storage;
using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Bw.VaultBot.Jobs;

public static class JobsExtensions
{
    public static string ToCsvFileName(this DateTime dateTime)
        => $"logins_{dateTime:yyyyMMddHHmmss}.csv";
    
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services)
        => services
            .AddTransient<IStorage>(sp => new FileStorage("./saved_files"))
            .AddScoped<ExportLoginsJob>();

    public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app, CancellationToken cancellationToken)
    {
        RecurringJob
            .AddOrUpdate<ExportLoginsJob>(
                "export-logins",
                job => job.Execute(cancellationToken),
                Cron.Daily(2));
        
        return app;
    }
}