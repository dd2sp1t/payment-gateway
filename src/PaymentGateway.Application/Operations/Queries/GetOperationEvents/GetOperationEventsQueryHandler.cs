using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Queries.GetOperationEvents;

internal sealed class GetOperationEventsQueryHandler
    : IRequestHandler<GetOperationEventsQuery, IReadOnlyList<OperationEventReadModel>>
{
    private readonly IOperationReadRepository _operationReadRepository;

    public GetOperationEventsQueryHandler(IOperationReadRepository operationReadRepository)
    {
        _operationReadRepository = operationReadRepository;
    }

    public async Task<IReadOnlyList<OperationEventReadModel>> Handle(
        GetOperationEventsQuery request,
        CancellationToken cancellationToken)
    {
        return await _operationReadRepository.GetOperationEventsAsync(
            (OperationId)request.OperationId,
            cancellationToken);
    }
}