using MediatR;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.SubmitOperation;

internal sealed class SubmitOperationCommandHandler : IRequestHandler<SubmitOperationCommand, SubmitOperationResponse>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOperationCommandHandler(IOperationRepository operationRepository, IUnitOfWork unitOfWork)
    {
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SubmitOperationResponse> Handle(
        SubmitOperationCommand request,
        CancellationToken cancellationToken)
    {
        var operation = await _operationRepository.GetAsync((OperationId)request.OperationId, cancellationToken);

        if (operation is null)
        {
            throw new NotFoundException($"Operation '{request.OperationId}' was not found.");
        }

        var newlyScheduled = false;

        if (operation.Status == OperationStatus.Created)
        {
            operation.StartProcessing();

            await _operationRepository.UpdateAsync(operation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            operation.ClearUncommittedEvents();

            newlyScheduled = true;
        }

        return new SubmitOperationResponse(operation.OperationId, operation.Status, newlyScheduled);
    }
}