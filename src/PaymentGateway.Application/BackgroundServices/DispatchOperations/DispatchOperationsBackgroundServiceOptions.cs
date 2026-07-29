namespace PaymentGateway.Application.BackgroundServices.DispatchOperations;

public sealed class DispatchOperationsBackgroundServiceOptions
{
    public TimeSpan Interval { get; init; }
    public int BatchSize { get; init; }
    public int MaxParallelDispatches { get; init; }
}