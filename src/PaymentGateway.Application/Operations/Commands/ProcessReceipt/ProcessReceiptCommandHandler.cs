using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.ProcessReceipt;

internal sealed class ProcessReceiptCommandHandler : IRequestHandler<ProcessReceiptCommand>
{
    private readonly ILogger<ProcessReceiptCommandHandler> _logger;
    private readonly IMetrics _metrics;
    private readonly IOperationReadRepository _operationReadRepository;
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ProcessReceiptCommandHandler(
        ILogger<ProcessReceiptCommandHandler> logger,
        IMetrics metrics,
        IOperationReadRepository operationReadRepository,
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _metrics = metrics;
        _operationReadRepository = operationReadRepository;
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(ProcessReceiptCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Receipt received. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} Result={Result}",
            request.OperationId,
            request.ProviderPaymentId,
            request.Result);

        var operationId = (OperationId)request.OperationId;

        var isProcessed = await _operationReadRepository.IsReceiptProcessedAsync(
            operationId,
            request.ProviderPaymentId,
            request.Result,
            cancellationToken);

        if (isProcessed)
        {
            _logger.LogInformation(
                "Receipt already processed. Skipping. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} Result={Result}",
                request.OperationId,
                request.ProviderPaymentId,
                request.Result);

            return;
        }

        var operation = await _operationRepository.GetAsync(operationId, cancellationToken);

        if (operation is null)
        {
            _logger.LogWarning(
                "Operation not found. OperationId={OperationId}",
                request.OperationId);

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
            _logger.LogInformation(
                "Receipt was processed concurrently. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId}",
                request.OperationId,
                request.ProviderPaymentId);

            return;
        }
        catch (DuplicateResourceException exception)
            when (exception.Resource == nameof(OperationEvent))
        {
            throw new ConcurrencyException("Optimistic concurrency conflict while processing receipt.");
        }

        switch (operation.Status)
        {
            case OperationStatus.Completed:
                _metrics.OperationCompleted();
                break;

            case OperationStatus.Rejected:
                _metrics.OperationRejected();
                break;
        }

        operation.ClearUncommittedEvents();
        operation.ClearUncommittedReceipts();

        _logger.LogInformation(
            "Receipt processed. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} Status={Status}",
            operation.OperationId,
            operation.ProviderPaymentId,
            operation.Status);
    }
}