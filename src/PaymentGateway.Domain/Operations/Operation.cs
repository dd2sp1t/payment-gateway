using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.Operations;

public sealed class Operation
{
    #region Fields

    private readonly List<OperationEvent> _uncommittedEvents = [];
    private readonly List<Receipt> _uncommittedReceipts = [];

    #endregion

    #region Properties

    public OperationId OperationId { get; }
    public Guid? ProviderPaymentId { get; private set; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Description { get; }
    public OperationStatus Status { get; private set; }
    public long LastEventId { get; private set; }
    public IReadOnlyList<OperationEvent> UncommittedEvents => _uncommittedEvents;
    public IReadOnlyList<Receipt> UncommittedReceipts => _uncommittedReceipts;

    #endregion

    #region Constructors

    private Operation(
        OperationId operationId,
        Guid? providerPaymentId,
        decimal amount,
        string currency,
        string description,
        OperationStatus status,
        long lastEventId)
    {
        OperationId = operationId;
        ProviderPaymentId = providerPaymentId;
        Amount = amount;
        Currency = currency;
        Description = description;
        Status = status;
        LastEventId = lastEventId;
    }

    #endregion

    #region Factory methods

    public static Operation Create(
        OperationId operationId,
        decimal amount,
        string currency,
        string description)
    {
        if (amount <= 0)
        {
            throw new DomainException("Operation amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new DomainException("Currency is required.");
        }

        if (string.Equals(currency, Currencies.Rub, StringComparison.Ordinal) == false)
        {
            throw new DomainException($"Currency '{currency}' is not supported.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("Description is required.");
        }

        var operation = new Operation(
            operationId,
            providerPaymentId: null,
            amount,
            currency,
            description,
            status: OperationStatus.Created,
            lastEventId: 0);

        operation.AddEvent(
            type: OperationEventType.Created,
            fromStatus: null,
            toStatus: OperationStatus.Created,
            message: "Operation created.");

        return operation;
    }

    internal static Operation Restore(
        OperationId operationId,
        Guid? providerPaymentId,
        decimal amount,
        string currency,
        string description,
        OperationStatus status,
        long lastEventId)
    {
        return new Operation(
            operationId,
            providerPaymentId,
            amount,
            currency,
            description,
            status,
            lastEventId);
    }

    #endregion

    #region Public methods

    public void Submit()
    {
        if (Status != OperationStatus.Created)
        {
            throw new DomainException(
                $"Operation '{OperationId}' cannot be moved from '{Status}' to '{OperationStatus.Processing}'.");
        }

        Status = OperationStatus.Processing;

        AddEvent(
            type: OperationEventType.Submited,
            fromStatus: OperationStatus.Created,
            toStatus: OperationStatus.Processing,
            message: "Operation processing started.");
    }

    public void ProcessReceipt(Receipt receipt)
    {
        SetProviderPaymentId(receipt.ProviderPaymentId);

        switch (Status, receipt.Result)
        {
            case (OperationStatus.Processing, ReceiptResult.Completed):
                Complete(receipt);
                break;

            case (OperationStatus.Processing, ReceiptResult.Rejected):
                Reject(receipt);
                break;

            case (OperationStatus.Completed, ReceiptResult.Rejected):
            case (OperationStatus.Rejected, ReceiptResult.Completed):
                Ignore(receipt);
                break;

            case (OperationStatus.Completed, ReceiptResult.Completed):
            case (OperationStatus.Rejected, ReceiptResult.Rejected):
                break;

            default:
                throw new DomainException(
                    $"Cannot process receipt for operation '{OperationId}' with status '{Status}'.");
        }
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    public void ClearUncommittedReceipts()
    {
        _uncommittedReceipts.Clear();
    }

    public void AttachProviderPayment(Guid providerPaymentId)
    {
        SetProviderPaymentId(providerPaymentId);
    }

    #endregion

    #region Private methods

    private void SetProviderPaymentId(Guid providerPaymentId)
    {
        if (ProviderPaymentId is null)
        {
            ProviderPaymentId = providerPaymentId;
            return;
        }

        if (ProviderPaymentId != providerPaymentId)
        {
            throw new ProviderPaymentMismatchException(OperationId);
        }
    }

    private void AddEvent(
        OperationEventType type,
        OperationStatus? fromStatus,
        OperationStatus toStatus,
        string message)
    {
        var @event = OperationEvent.Create(OperationId, ++LastEventId, type, fromStatus, toStatus, message);
        _uncommittedEvents.Add(@event);
    }

    private void AddReceipt(Receipt receipt)
    {
        _uncommittedReceipts.Add(receipt);
    }

    private void Complete(Receipt receipt)
    {
        AddReceipt(receipt);

        Status = OperationStatus.Completed;

        AddEvent(
            type: OperationEventType.Completed,
            fromStatus: OperationStatus.Processing,
            toStatus: OperationStatus.Completed,
            message: "Operation completed.");
    }

    private void Reject(Receipt receipt)
    {
        AddReceipt(receipt);

        Status = OperationStatus.Rejected;

        AddEvent(
            type: OperationEventType.Rejected,
            fromStatus: OperationStatus.Processing,
            toStatus: OperationStatus.Rejected,
            message: "Operation rejected.");
    }

    private void Ignore(Receipt receipt)
    {
        AddReceipt(receipt);

        AddEvent(
            type: OperationEventType.Ignored,
            fromStatus: Status,
            toStatus: Status,
            message: "Ignored conflicting receipt.");
    }

    #endregion
}