using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.Dispatch;

namespace PaymentGateway.Infrastructure.Dispatch;

internal sealed class ExponentialBackoffDispatchRetryPolicy : IDispatchRetryPolicy
{
    private readonly DispatchOptions _options;

    public ExponentialBackoffDispatchRetryPolicy(IOptions<DispatchOptions> options)
    {
        _options = options.Value;
    }

    public bool CanRetry(int retryCount)
    {
        return retryCount < _options.RetryCount;
    }

    public DateTimeOffset GetNextDispatchAt(int attempts)
    {
        if (CanRetry(attempts) == false)
        {
            throw new InvalidOperationException("Retry limit has been reached.");
        }

        var delaySeconds = _options.BaseDelaySeconds * Math.Pow(2, attempts);

        var jitter = Random.Shared.Next(_options.MinJitterMilliseconds, _options.MaxJitterMilliseconds + 1);

        return DateTimeOffset.UtcNow
            .AddSeconds(delaySeconds)
            .AddMilliseconds(jitter);
    }
}