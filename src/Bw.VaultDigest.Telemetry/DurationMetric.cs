using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Bw.VaultDigest.Telemetry;

public static class DurationCounterExtensions
{
    public static DurationMetric CreateDurationCounter(this Histogram<long> histogram)
    {
        return new DurationMetric(histogram);
    }
}

public class DurationMetric(Histogram<long> histogram) : IDisposable
{
    private bool _disposed = false;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    
    ~DurationMetric()
    {
        if (!_disposed)
            Dispose();
    }
    
    public void Dispose()
    {
        _stopwatch.Stop();
        histogram.Record(_stopwatch.ElapsedMilliseconds);
    }
}