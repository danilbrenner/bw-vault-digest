using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Bw.VaultDigest.Telemetry;

public static class TelemetryConstants
{
    public const string OpenTelemetryServiceName = "VaultDigest";
}

public static class Setup
{
    public static IServiceCollection AddTelemetry(this IServiceCollection svc)
    {
        svc
            .AddOpenTelemetry()
            .ConfigureResource(r => r.AddService(TelemetryConstants.OpenTelemetryServiceName))
            .WithMetrics(m =>
            {
                m
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("VaultDigest")
                    .AddOtlpExporter();
            }).WithTracing(t =>
            {
                t
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter();
            });

        return svc.AddTransient<MetricsFactory>();
    }
}