using MediatR;

namespace PaymentGateway.Application.Operations.SubmitOperation;

public sealed record SubmitOperationCommand(string OperationId) : IRequest<SubmitOperationResponse>;