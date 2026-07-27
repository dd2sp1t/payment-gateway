using MediatR;
using PaymentGateway.Application.Operations.Models;

namespace PaymentGateway.Application.Operations.Queries.GetOperation;

public sealed record GetOperationQuery(string OperationId) : IRequest<OperationResponse>;