namespace PaymentGateway.DemoRunner;

internal sealed class DemoRunnerOptions
{
    public string GatewayUrl { get; init; } = null!;
    public int Operations { get; init; }
    public int MaxParallelOperations { get; init; }
    public int ConcurrentSubmits { get; init; }
    public TimeSpan ReceiptProcessingWait { get; init; }
    public TimeSpan PauseBetweenOperations { get; init; }
}