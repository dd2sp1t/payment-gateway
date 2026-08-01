using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Models;

public sealed record OperationResponse(
    string OperationId,
    string Amount,
    string Currency,
    string Description,
    OperationStatus Status,
    Guid? ProviderPaymentId,
    int RetryCount,
    DateTimeOffset? NextDispatchAt);