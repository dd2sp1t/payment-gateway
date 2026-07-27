using MediatR;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.SubmitOperation;

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

        (operation, newlyScheduled) = await SubmitIfCreatedAsync(operation, cancellationToken);

        return new SubmitOperationResponse(operation.OperationId, operation.Status, newlyScheduled);
    }

    private async Task<(Operation Operation, bool NewlyScheduled)> SubmitIfCreatedAsync(
        Operation operation,
        CancellationToken cancellationToken)
    {
        if (operation.Status != OperationStatus.Created)
        {
            return (operation, NewlyScheduled: false);
        }

        try
        {
            operation.Submit();

            await _operationRepository.UpdateAsync(operation, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            operation.ClearUncommittedEvents();

            return (operation, NewlyScheduled: true);
        }
        catch (ConcurrencyException)
        {
            var actual = await _operationRepository.GetAsync(operation.OperationId, cancellationToken);

            return (actual!, NewlyScheduled: false);
        }
    }
}