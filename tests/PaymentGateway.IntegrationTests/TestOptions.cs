using PaymentGateway.Application.BackgroundServices.DispatchOperations;
using PaymentGateway.Infrastructure.Dispatch;

namespace PaymentGateway.IntegrationTests;

internal static class TestOptions
{
    public const int DispatchMaxRetryCount = 3;
    public static TimeSpan StabilityDelay => TimeSpan.FromSeconds(6);

    public static readonly DispatchOptions Dispatch = new()
    {
        MaxRetryCount = DispatchMaxRetryCount,
        BaseDelaySeconds = 0,
        MinJitterMilliseconds = 0,
        MaxJitterMilliseconds = 0
    };

    public static readonly DispatchOperationsBackgroundServiceOptions Background = new()
    {
        Interval = TimeSpan.FromMilliseconds(500),
        BatchSize = 100,
        MaxParallelDispatches = 10
    };
}