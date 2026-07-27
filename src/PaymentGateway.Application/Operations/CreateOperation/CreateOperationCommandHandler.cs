using System.Globalization;
using MediatR;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Operations.Models;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.CreateOperation;

internal sealed class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, OperationResponse>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOperationCommandHandler(
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResponse> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = Operation.Create(
            (OperationId)request.OperationId,
            amount: decimal.Parse(request.Amount, NumberStyles.Number, CultureInfo.InvariantCulture),
            request.Currency,
            request.Description);

        _operationRepository.Add(operation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        operation.ClearUncommittedEvents();

        return new OperationResponse(
            operation.OperationId,
            operation.Amount.ToInvariantString(),
            operation.Currency,
            operation.Description,
            operation.Status,
            operation.ProviderPaymentId);
    }
}