using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public sealed record OperationReadModel(
    OperationId OperationId,
    decimal Amount,
    string Currency,
    string Description,
    OperationStatus Status,
    Guid? ProviderPaymentId,
    int RetryCount,
    DateTimeOffset? NextDispatchAt);