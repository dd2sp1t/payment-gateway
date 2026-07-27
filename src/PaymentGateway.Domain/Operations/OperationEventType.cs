namespace PaymentGateway.Domain.Operations;

public enum OperationEventType
{
    Created,
    Submited,
    Completed,
    Rejected,
    Ignored
}