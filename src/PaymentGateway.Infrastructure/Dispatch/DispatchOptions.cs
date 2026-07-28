namespace PaymentGateway.Infrastructure.Dispatch;

public sealed class DispatchOptions
{
    public int MaxRetryCount { get; init; }
    public int BaseDelaySeconds { get; init; }
    public int MinJitterMilliseconds { get; init; }
    public int MaxJitterMilliseconds { get; init; }
}