using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;

namespace PaymentGateway.Application.Operations.Queries.GetOperationEvents;

public sealed record GetOperationEventsQuery(string OperationId) : IRequest<IReadOnlyList<OperationEventReadModel>>;