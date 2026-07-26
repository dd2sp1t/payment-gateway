namespace PaymentGateway.Domain.Operations;

public enum OperationEventType
{
    Created,
    Processing,
    Completed,
    Rejected,
    Ignored
}