using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.Operations;

public sealed class Operation
{
    #region Fields

    private readonly List<OperationEvent> _uncommittedEvents = [];

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
            message: "Operation created");

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

    public void StartProcessing()
    {
        if (Status != OperationStatus.Created)
        {
            throw new DomainException(
                $"Operation '{OperationId}' cannot be moved from '{Status}' to '{OperationStatus.Processing}'.");
        }

        Status = OperationStatus.Processing;

        AddEvent(
            type: OperationEventType.Processing,
            fromStatus: OperationStatus.Created,
            toStatus: OperationStatus.Processing,
            message: "Operation processing started");
    }

    public void Complete(Guid providerPaymentId)
    {
        if (Status != OperationStatus.Processing)
        {
            throw new DomainException($"Operation '{OperationId}' cannot be completed from '{Status}'.");
        }

        SetProviderPaymentId(providerPaymentId);

        Status = OperationStatus.Completed;

        AddEvent(
            type: OperationEventType.Completed,
            fromStatus: OperationStatus.Processing,
            toStatus: OperationStatus.Completed,
            message: "Operation completed");
    }

    public void Reject(Guid providerPaymentId)
    {
        if (Status != OperationStatus.Processing)
        {
            throw new DomainException($"Operation '{OperationId}' cannot be rejected from '{Status}'.");
        }

        SetProviderPaymentId(providerPaymentId);

        Status = OperationStatus.Rejected;

        AddEvent(
            type: OperationEventType.Rejected,
            fromStatus: OperationStatus.Processing,
            toStatus: OperationStatus.Rejected,
            message: "Operation rejected");
    }

    public void ClearUncommittedEvents()
    {
        _uncommittedEvents.Clear();
    }

    #endregion

    #region Private methods

    private void SetProviderPaymentId(Guid providerPaymentId)
    {
        if (ProviderPaymentId is not null && ProviderPaymentId != providerPaymentId)
        {
            throw new DomainException(
                $"Provider payment id cannot be changed for operation '{OperationId}'.");
        }

        ProviderPaymentId ??= providerPaymentId;
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

    #endregion
}