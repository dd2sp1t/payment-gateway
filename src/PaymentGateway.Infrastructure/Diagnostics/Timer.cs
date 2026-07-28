using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace PaymentGateway.Infrastructure.Diagnostics;

internal sealed class Timer : IDisposable
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly Histogram<double> _histogram;
    private readonly TagList _tags;

    public Timer(Histogram<double> histogram)
    {
        _histogram = histogram;
        _tags = default;
    }

    public Timer(Histogram<double> histogram, string tagName, string tagValue)
    {
        _histogram = histogram;
        _tags = new TagList
            {
                { tagName, tagValue }
            };
    }

    public void Dispose()
    {
        _histogram.Record(_stopwatch.Elapsed.TotalMilliseconds, _tags);
    }
}
