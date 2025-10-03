using Hangfire;
using Microsoft.AspNetCore.Builder;

namespace Bw.VaultBot.Jobs;

public static class JobsExtensions
{
    public static IServiceCollection AddHangfireJobs(this IServiceCollection services)
        => services.AddScoped<ExportLoginsJob>();

    public static IApplicationBuilder UseHangfireJobs(this IApplicationBuilder app)
    {
        RecurringJob.AddOrUpdate<ExportLoginsJob>("export-logins", job => job.Execute(), Cron.Daily(2));
        return app;
    }
}