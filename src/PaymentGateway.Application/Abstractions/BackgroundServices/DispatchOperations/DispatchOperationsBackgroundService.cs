using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Operations.DispatchOperation;

namespace PaymentGateway.Application.Abstractions.BackgroundServices.DispatchOperations;

internal sealed class DispatchOperationsBackgroundService : PeriodicBackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DispatchOperationsBackgroundServiceOptions _options;

    public DispatchOperationsBackgroundService(
        ILogger<DispatchOperationsBackgroundService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<DispatchOperationsBackgroundServiceOptions> options)
        : base(logger, options.Value.Interval)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteIterationAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var reader = scope.ServiceProvider.GetRequiredService<IOperationReadRepository>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var operations = await reader.GetProcessingOperationsAsync(_options.BatchSize, cancellationToken);

        foreach (var chunk in operations.Chunk(_options.MaxParallelDispatches))
        {
            var tasks = chunk
                .Select(x => mediator.Send(new DispatchOperationCommand(x.OperationId), cancellationToken));

            await Task.WhenAll(tasks);
        }
    }
}