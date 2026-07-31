using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Queries.GetReceipts;

internal sealed class GetReceiptsQueryHandler : IRequestHandler<GetReceiptsQuery, IReadOnlyList<ReceiptReadModel>>
{
    private readonly IOperationReadRepository _operationReadRepository;

    public GetReceiptsQueryHandler(IOperationReadRepository operationReadRepository)
    {
        _operationReadRepository = operationReadRepository;
    }

    public async Task<IReadOnlyList<ReceiptReadModel>> Handle(
        GetReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        return await _operationReadRepository.GetReceiptsAsync(
            (OperationId)request.OperationId,
            cancellationToken);
    }
}