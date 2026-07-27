using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.Operations;

public sealed class Receipt
{
    #region Properties

    public Guid ReceiptId { get; }
    public Guid ProviderPaymentId { get; }
    public OperationId OperationId { get; }
    public ReceiptResult Result { get; }
    public string Message { get; }
    public DateTimeOffset OccurredAt { get; }

    #endregion

    #region Constructors

    private Receipt(
        Guid providerPaymentId,
        OperationId operationId,
        ReceiptResult result,
        string message,
        DateTimeOffset occurredAt)
    {
        ReceiptId = Guid.NewGuid();
        ProviderPaymentId = providerPaymentId;
        OperationId = operationId;
        Result = result;
        Message = message;
        OccurredAt = occurredAt;
    }

    #endregion

    #region Factory methods

    public static Receipt Create(
        Guid providerPaymentId,
        OperationId operationId,
        ReceiptResult result,
        string message,
        DateTimeOffset occurredAt
    )
    {
        if (providerPaymentId == Guid.Empty)
        {
            throw new DomainException("Provider payment id is required.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new DomainException("Receipt message is required.");
        }

        if (occurredAt == default)
        {
            throw new DomainException("Receipt occurrence time is required.");
        }

        return new Receipt(providerPaymentId, operationId, result, message, occurredAt);
    }

    #endregion
}