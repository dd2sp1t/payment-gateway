using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public sealed record OperationDispatchModel(
    OperationId OperationId,
    decimal Amount,
    string Currency,
    Guid? ProviderPaymentId);