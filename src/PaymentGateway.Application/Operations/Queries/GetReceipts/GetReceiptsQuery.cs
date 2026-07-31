using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Application.Abstractions.Requests;

namespace PaymentGateway.Application.Operations.Queries.GetReceipts;

public sealed record GetReceiptsQuery(string OperationId)
    : IRequest<IReadOnlyList<ReceiptReadModel>>, IOperationRequest;