using System.Diagnostics.Metrics;
using OpenTelemetry.Metrics;

namespace Bw.VaultBot.Telemetry;

public class MetricsFactory(IMeterFactory meterFactory) : IDisposable
{
    private bool _disposed;
    private readonly Meter _meter = meterFactory.Create(TelemetryConstants.OpenTelemetryServiceName);

    public DurationMetric CreateDurationMetric(string metricName)
    {
        return _meter.CreateHistogram<long>(metricName).CreateDurationCounter();
    }

    ~MetricsFactory()
    {
        if (!_disposed)
            Dispose();
    }

    public void Dispose()
    {
        _meter.Dispose();
        _disposed = true;
    }
}