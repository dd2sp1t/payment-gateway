using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Requests;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Helpers;

namespace PaymentGateway.Application.Behaviors;

internal sealed class OptimisticConcurrencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IOptimisticConcurrencyRequest, IOperationRequest
{
    private const int MaxRetries = 20;

    private readonly ILogger<OptimisticConcurrencyBehavior<TRequest, TResponse>> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMetrics _metrics;

    public OptimisticConcurrencyBehavior(
        ILogger<OptimisticConcurrencyBehavior<TRequest, TResponse>> logger,
        IUnitOfWork unitOfWork,
        IMetrics metrics)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _metrics = metrics;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                return await next();
            }
            catch (ConcurrencyException) when (attempt < MaxRetries)
            {
                var requestName = RequestNameHelper.GetName<TRequest>();
                _metrics.ApplicationRequestConcurrencyRetry(requestName);

                _unitOfWork.ClearChangeTracker();

                _logger.LogDebug(
                    "Concurrency conflict. OperationId={OperationId} Request={Request} Attempt={Attempt}/{MaxAttempts}",
                    (request as IOperationRequest)?.OperationId,
                    requestName,
                    attempt,
                    MaxRetries);
            }
        }

        throw new ConcurrencyException($"Optimistic concurrency retry limit ({MaxRetries}) reached.");
    }
}