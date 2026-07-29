using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public sealed record ProcessingOperationReadModel(
    OperationId OperationId,
    OperationStatus Status,
    DateTimeOffset? NextDispatchAt);