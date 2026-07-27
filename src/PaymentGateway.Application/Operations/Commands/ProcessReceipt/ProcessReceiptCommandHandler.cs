using MediatR;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.ProcessReceipt;

internal sealed class ProcessReceiptCommandHandler : IRequestHandler<ProcessReceiptCommand>
{
    private readonly IOperationReadRepository _operationReadRepository;
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReceiptCommandHandler(
        IOperationReadRepository operationReadRepository,
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _operationReadRepository = operationReadRepository;
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessReceiptCommand request, CancellationToken cancellationToken)
    {
        var operationId = (OperationId)request.OperationId;

        var isProcessed = await _operationReadRepository.IsReceiptProcessedAsync(
            operationId,
            request.ProviderPaymentId,
            request.Result,
            cancellationToken);

        if (isProcessed)
        {
            return;
        }

        var operation = await _operationRepository.GetAsync(
            operationId,
            cancellationToken);

        if (operation is null)
        {
            throw new NotFoundException($"Operation '{request.OperationId}' was not found.");
        }

        var receipt = Receipt.Create(
            request.ProviderPaymentId,
            operationId,
            request.Result,
            request.Message,
            request.OccurredAt);

        operation.ProcessReceipt(receipt);

        await _operationRepository.UpdateAsync(operation, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DuplicateResourceException exception)
            when (exception.Resource == nameof(Receipt))
        {
            return;
        }

        operation.ClearUncommittedEvents();
        operation.ClearUncommittedReceipts();
    }
}