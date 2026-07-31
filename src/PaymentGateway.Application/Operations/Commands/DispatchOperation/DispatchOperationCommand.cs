using MediatR;
using PaymentGateway.Application.Abstractions.Requests;

namespace PaymentGateway.Application.Operations.Commands.DispatchOperation;

public sealed record DispatchOperationCommand(string OperationId)
    : IRequest, IOperationRequest, IOptimisticConcurrencyRequest;