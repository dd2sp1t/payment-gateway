using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Operations.Commands.DispatchOperation;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.BackgroundServices.DispatchOperations;

internal sealed class DispatchOperationsBackgroundService : PeriodicBackgroundService
{
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
        _metrics = metrics;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteIterationAsync(CancellationToken cancellationToken)
    {
        using var _ = _metrics.MeasureDispatchBatch();

        using var scope = _scopeFactory.CreateScope();

        var reader = scope.ServiceProvider.GetRequiredService<IOperationReadRepository>();

        var ids = await reader.GetProcessingOperationIdsAsync(_options.BatchSize, cancellationToken);

        var oldestAge = await reader.GetOldestProcessingAgeAsync(cancellationToken);

        _metrics.ProcessingOldestAge(oldestAge);

        foreach (var chunk in ids.Chunk(_options.MaxParallelDispatches))
        {
            var tasks = chunk.Select(id => DispatchOperationAsync(id, cancellationToken));

            await Task.WhenAll(tasks);
        }

        _metrics.DispatchBatch(ids.Count);
    }

    private async Task DispatchOperationAsync(OperationId operationId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        await mediator.Send(new DispatchOperationCommand(operationId), cancellationToken);
    }
}