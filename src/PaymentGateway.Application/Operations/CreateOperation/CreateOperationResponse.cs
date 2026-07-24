using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.CreateOperation;

public sealed record CreateOperationResponse(
    string OperationId,
    OperationStatus Status);