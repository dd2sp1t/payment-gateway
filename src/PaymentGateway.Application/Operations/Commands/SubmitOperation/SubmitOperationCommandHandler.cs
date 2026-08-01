using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.SubmitOperation;

internal sealed class SubmitOperationCommandHandler : IRequestHandler<SubmitOperationCommand, SubmitOperationResponse>
{
    private readonly ILogger<SubmitOperationCommandHandler> _logger;
    private readonly IMetrics _metrics;
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitOperationCommandHandler(
        ILogger<SubmitOperationCommandHandler> logger,
        IMetrics metrics,
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _metrics = metrics;
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
            _logger.LogWarning(
                "Operation not found. Skipping. OperationId={OperationId}",
                request.OperationId);

            throw new NotFoundException($"Operation '{request.OperationId}' was not found.");
        }

        bool newlyScheduled;
        (operation, newlyScheduled) = await SubmitIfCreatedAsync(operation, cancellationToken);

        return new SubmitOperationResponse(operation.OperationId, operation.Status, newlyScheduled);
    }

    private async Task<(Operation Operation, bool NewlyScheduled)> SubmitIfCreatedAsync(
        Operation operation,
        CancellationToken cancellationToken)
    {
        if (operation.Status != OperationStatus.Created)
        {
            _logger.LogInformation(
                "Operation status is invalid. Skipping. OperationId={OperationId} Status={Status}",
                operation.OperationId,
                operation.Status);

            return (operation, NewlyScheduled: false);
        }

        try
        {
            operation.Submit();

            await _operationRepository.UpdateAsync(operation, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _metrics.OperationSubmitted();

            _logger.LogInformation(
                "Operation submitted. OperationId={OperationId}",
                operation.OperationId);

            operation.ClearUncommittedEvents();

            return (operation, NewlyScheduled: true);
        }
        catch (DuplicateResourceException exception)
        {
            throw new ConcurrencyException(
                "Optimistic concurrency conflict while submitting operation.",
                exception);
        }
    }
}