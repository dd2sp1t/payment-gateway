using MediatR;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Operations.Models;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Queries.GetOperation;

internal sealed class GetOperationQueryHandler : IRequestHandler<GetOperationQuery, OperationResponse>
{
    private readonly IOperationReadRepository _operationReadRepository;

    public GetOperationQueryHandler(IOperationReadRepository operationReadRepository)
    {
        _operationReadRepository = operationReadRepository;
    }

    public async Task<OperationResponse> Handle(GetOperationQuery request, CancellationToken cancellationToken)
    {
        var operation = await _operationReadRepository.GetOperationAsync(
            (OperationId)request.OperationId,
            cancellationToken);

        if (operation is null)
        {
            throw new NotFoundException($"Operation '{request.OperationId}' was not found.");
        }

        return new OperationResponse(
            operation.OperationId,
            operation.Amount.ToInvariantString(),
            operation.Currency,
            operation.Description,
            operation.Status,
            operation.ProviderPaymentId,
            operation.RetryCount,
            operation.NextDispatchAt);
    }
}