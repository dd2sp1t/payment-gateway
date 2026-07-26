using MediatR;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.DispatchOperation;

public sealed record DispatchOperationCommand(OperationId OperationId) : IRequest;