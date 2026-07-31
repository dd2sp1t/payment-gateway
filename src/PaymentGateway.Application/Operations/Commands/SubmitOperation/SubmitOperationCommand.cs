using MediatR;

namespace PaymentGateway.Application.Operations.Commands.SubmitOperation;

public sealed record SubmitOperationCommand(string OperationId)
    : IRequest<SubmitOperationResponse>, IOperationRequest;