using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Application.Abstractions.Requests;

namespace PaymentGateway.Application.Operations.Queries.GetOperationEvents;

public sealed record GetOperationEventsQuery(string OperationId)
    : IRequest<IReadOnlyList<OperationEventReadModel>>, IOperationRequest;