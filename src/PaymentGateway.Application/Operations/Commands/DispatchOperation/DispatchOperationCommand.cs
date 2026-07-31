using MediatR;

namespace PaymentGateway.Application.Operations.Commands.DispatchOperation;

public sealed record DispatchOperationCommand(string OperationId)
    : IRequest, IOperationRequest;