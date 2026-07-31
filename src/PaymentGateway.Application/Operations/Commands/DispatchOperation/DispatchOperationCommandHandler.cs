using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Dispatch;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.DispatchOperation;

internal sealed class DispatchOperationCommandHandler : IRequestHandler<DispatchOperationCommand>
{
    private const int MaxConcurrencyRetries = 20;
    private readonly ILogger<DispatchOperationCommandHandler> _logger;
    private readonly IMetrics _metrics;
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderClient _paymentProviderClient;
    private readonly IDispatchRetryPolicy _dispatchRetryPolicy;
    private readonly IDispatchFailureClassifier _dispatchFailureClassifier;

    public DispatchOperationCommandHandler(
        ILogger<DispatchOperationCommandHandler> logger,
        IMetrics metrics,
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork,
        IPaymentProviderClient paymentProviderClient,
        IDispatchRetryPolicy dispatchRetryPolicy,
        IDispatchFailureClassifier dispatchFailureClassifier)
    {
        _logger = logger;
        _metrics = metrics;
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
        _paymentProviderClient = paymentProviderClient;
        _dispatchRetryPolicy = dispatchRetryPolicy;
        _dispatchFailureClassifier = dispatchFailureClassifier;
    }

    public async Task Handle(DispatchOperationCommand request, CancellationToken cancellationToken)
    {
        for (var concurrencyAttempt = 0; concurrencyAttempt <= MaxConcurrencyRetries; concurrencyAttempt++)
        {
            var operation = await _operationRepository.GetAsync(
                (OperationId)request.OperationId,
                cancellationToken);

            if (operation is null)
            {
                _logger.LogWarning(
                    "Operation not found. OperationId={OperationId}",
                    request.OperationId);

                return;
            }

            if (operation.Status != OperationStatus.Processing)
            {
                _logger.LogDebug(
                    "Dispatch skipped. OperationId={OperationId} Status={Status} RetryCount={RetryCount} NextDispatchAt={NextDispatchAt}",
                    operation.OperationId,
                    operation.Status,
                    operation.RetryCount,
                    operation.NextDispatchAt);

                return;
            }

            try
            {
                await DispatchAsync(operation, cancellationToken);

                return;
            }
            catch (ConcurrencyException)
            {
                _metrics.DispatchConcurrencyRetry();

                _logger.LogInformation(
                    "Concurrency conflict. Reloading operation. OperationId={OperationId} Attempt={Attempt} MaxAttempts={MaxAttempts}",
                    operation.OperationId,
                    concurrencyAttempt + 1,
                    MaxConcurrencyRetries);
            }
        }

        throw new ConcurrencyException("Optimistic concurrency retry limit was reached while dispatching the operation.");
    }

    private async Task DispatchAsync(Operation operation, CancellationToken cancellationToken)
    {
        var request = new SubmitPaymentRequest(
            operation.OperationId,
            operation.Amount.ToInvariantString(),
            operation.Currency);

        try
        {
            _metrics.DispatchRequested();

            var response = await _paymentProviderClient.SubmitAsync(
                request,
                operation.RetryCount,
                cancellationToken);

            operation.AttachProviderPayment(response.ProviderPaymentId);

            await PersistAsync(operation, cancellationToken);

            _metrics.DispatchSucceeded();

            _logger.LogInformation(
                "Dispatch succeeded. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} RetryCount={RetryCount}",
                operation.OperationId,
                response.ProviderPaymentId,
                operation.RetryCount);
        }
        catch (Exception exception) when (_dispatchFailureClassifier.IsTransient(exception))
        {
            await HandleTransientFailureAsync(operation, exception, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Dispatch failed. OperationId={OperationId} RetryCount={RetryCount}",
                operation.OperationId,
                operation.RetryCount);

            throw;
        }
    }

    private async Task HandleTransientFailureAsync(
        Operation operation,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (_dispatchRetryPolicy.CanRetry(operation.RetryCount) == false)
        {
            operation.StopRetrying();

            await PersistAsync(operation, cancellationToken);

            _metrics.DispatchRetryLimitReached();

            _logger.LogError(
                exception,
                "Dispatch failed. OperationId={OperationId} RetryCount={RetryCount}",
                operation.OperationId,
                operation.RetryCount);

            return;
        }

        var nextDispatchAt = _dispatchRetryPolicy.GetNextDispatchAt(operation.RetryCount);

        operation.ScheduleRetry(nextDispatchAt);

        await PersistAsync(operation, cancellationToken);

        _metrics.DispatchRetryScheduled();

        _logger.LogWarning(
            exception,
            "Dispatch retry scheduled. OperationId={OperationId} RetryCount={RetryCount} NextDispatchAt={NextDispatchAt}",
            operation.OperationId,
            operation.RetryCount,
            operation.NextDispatchAt);
    }

    private async Task PersistAsync(Operation operation, CancellationToken cancellationToken)
    {
        await _operationRepository.UpdateAsync(operation, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        operation.ClearUncommittedEvents();
        operation.ClearUncommittedReceipts();
    }
}