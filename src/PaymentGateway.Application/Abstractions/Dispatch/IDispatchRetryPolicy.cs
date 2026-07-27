namespace PaymentGateway.Application.Abstractions.Dispatch;

public interface IDispatchRetryPolicy
{
    bool CanRetry(int attempt);
    DateTimeOffset GetNextDispatchAt(int attempt);
}