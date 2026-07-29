namespace PaymentGateway.Domain.Operations;

public enum OperationEventType
{
    Created,
    Submitted,
    Completed,
    Rejected,
    Ignored
}