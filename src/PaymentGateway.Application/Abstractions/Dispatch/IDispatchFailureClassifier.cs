namespace PaymentGateway.Application.Abstractions.Dispatch;

public interface IDispatchFailureClassifier
{
    bool IsTransient(Exception exception);
}