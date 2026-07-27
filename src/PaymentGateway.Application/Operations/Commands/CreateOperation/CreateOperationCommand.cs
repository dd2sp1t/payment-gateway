using MediatR;
using PaymentGateway.Application.Operations.Models;

namespace PaymentGateway.Application.Operations.Commands.CreateOperation;

public sealed record CreateOperationCommand(
    string OperationId,
    string Amount,
    string Currency,
    string Description)
    : IRequest<OperationResponse>;