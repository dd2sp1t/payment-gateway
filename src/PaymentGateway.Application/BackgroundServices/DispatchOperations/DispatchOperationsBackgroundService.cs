using System.Text.Json;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Operations.Commands.DispatchOperation;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.BackgroundServices.DispatchOperations;

internal sealed class DispatchOperationsBackgroundService : PeriodicBackgroundService
{
    private readonly ILogger<DispatchOperationsBackgroundService> _logger;
    private readonly IMetrics _metrics;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DispatchOperationsBackgroundServiceOptions _options;

    public DispatchOperationsBackgroundService(
        ILogger<DispatchOperationsBackgroundService> logger,
        IMetrics metrics,
        IServiceScopeFactory scopeFactory,
        IOptions<DispatchOperationsBackgroundServiceOptions> options)
        : base(logger, options.Value.Interval)
    {
        _logger = logger;
        _metrics = metrics;
        _scopeFactory = scopeFactory;
        _options = options.Value;

        _logger.LogInformation(
            "Service configured. ServiceName={ServiceName} Options={Options} HashCode={HashCode}",
            nameof(DispatchOperationsBackgroundService),
            JsonSerializer.Serialize(_options),
            GetHashCode());
    }

    protected override async Task ExecuteIterationAsync(CancellationToken cancellationToken)
    {
        using var _ = _metrics.MeasureDispatchBatch();

        using var scope = _scopeFactory.CreateScope();

        var reader = scope.ServiceProvider.GetRequiredService<IOperationReadRepository>();

        var ids = await reader.GetProcessingOperationIdsAsync(_options.BatchSize, cancellationToken);

        if (ids.Count == 0)
        {
            _logger.LogDebug(
                "No operations to dispatch. ServiceName={ServiceName} HashCode={HashCode}",
                nameof(DispatchOperationsBackgroundService),
                GetHashCode());
        }

        var oldestAge = await reader.GetOldestProcessingAgeAsync(cancellationToken);

        _metrics.ProcessingOldestAge(oldestAge);

        foreach (var chunk in ids.Chunk(_options.MaxParallelDispatches))
        {
            var tasks = chunk.Select(id => TryDispatchOperationAsync(id, cancellationToken));

            await Task.WhenAll(tasks);
        }

        _metrics.DispatchBatch(ids.Count);
    }

    private async Task TryDispatchOperationAsync(OperationId operationId, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.Send(
                new DispatchOperationCommand(operationId),
                cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Dispatch failed. OperationId={OperationId}",
                operationId);
        }
    }
}