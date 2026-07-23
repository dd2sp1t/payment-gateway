namespace PaymentGateway.Domain.Operations;

public enum OperationStatus
{
    Created,
    Processing,
    Completed,
    Rejected
}