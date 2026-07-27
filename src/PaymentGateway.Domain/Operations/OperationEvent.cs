namespace PaymentGateway.Domain.Operations;

public sealed class OperationEvent
{
    #region Properties

    public OperationId OperationId { get; }
    public long EventId { get; }
    public OperationEventType Type { get; }
    public OperationStatus? FromStatus { get; }
    public OperationStatus ToStatus { get; }
    public string Message { get; }
    public DateTimeOffset OccurredAt { get; }

    #endregion

    #region Constructors

    private OperationEvent(
        OperationId operationId,
        long eventId,
        OperationEventType type,
        OperationStatus? fromStatus,
        OperationStatus toStatus,
        string message)
    {
        OperationId = operationId;
        EventId = eventId;
        Type = type;
        FromStatus = fromStatus;
        ToStatus = toStatus;
        Message = message;
        OccurredAt = DateTimeOffset.UtcNow;
    }

    #endregion

    #region Factory methods

    internal static OperationEvent Create(
        OperationId operationId,
        long eventId,
        OperationEventType type,
        OperationStatus? fromStatus,
        OperationStatus toStatus,
        string message)
    {
        return new OperationEvent(
            operationId,
            eventId,
            type,
            fromStatus,
            toStatus,
            message);
    }

    #endregion
}