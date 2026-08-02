using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class ConcurrentSubmitScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;

    public string Name => nameof(DemoScenario.Concurrent);

    public ConcurrentSubmitScenario(
        PaymentGatewayClient client,
        IOptions<DemoRunnerOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await Parallel.ForEachAsync(
            Enumerable.Range(0, _options.Operations),
            new ParallelOptions
            {
                MaxDegreeOfParallelism = _options.MaxParallelOperations,
                CancellationToken = cancellationToken
            },
            async (_, ct) =>
            {
                await RunConcurrentSubmitAsync(ct);
            });
    }

    private async Task RunConcurrentSubmitAsync(CancellationToken cancellationToken)
    {
        var operationId = $"demo-runner-op-concurrent-{Guid.NewGuid()}";

        await _client.CreateOperationAsync(
            operationId,
            amount: "1000.00",
            currency: "RUB",
            description: "concurrent",
            cancellationToken);

        var submits = Enumerable
            .Range(0, _options.ConcurrentSubmits)
            .Select(_ => _client.SubmitOperationAsync(
                operationId,
                cancellationToken));

        await Task.WhenAll(submits);

        await Task.Delay(
            _options.ReceiptProcessingWait,
            cancellationToken);

        await _client.GetEventsAsync(
            operationId,
            cancellationToken);

        await _client.GetReceiptsAsync(
            operationId,
            cancellationToken);

        await _client.GetOperationAsync(
            operationId,
            cancellationToken);
    }
}