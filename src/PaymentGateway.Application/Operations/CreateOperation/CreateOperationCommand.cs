using MediatR;

namespace PaymentGateway.Application.Operations.CreateOperation;

public sealed record CreateOperationCommand(
    string OperationId,
    decimal Amount,
    string Currency,
    string Description)
    : IRequest<CreateOperationResponse>;