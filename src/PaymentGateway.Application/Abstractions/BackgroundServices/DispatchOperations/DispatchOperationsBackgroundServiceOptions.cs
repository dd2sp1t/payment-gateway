namespace PaymentGateway.Application.Abstractions.BackgroundServices.DispatchOperations;

public sealed class DispatchOperationsBackgroundServiceOptions
{
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(5);
    public int BatchSize { get; init; } = 100;
    public int MaxParallelDispatches { get; init; } = 10;
}