using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public sealed record OperationEventReadModel(
    long EventId,
    OperationEventType Type,
    OperationStatus? FromStatus,
    OperationStatus ToStatus,
    string Message,
    DateTimeOffset OccurredAt);