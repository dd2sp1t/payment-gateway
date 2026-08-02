using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class BasicFlowScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;

    public string Name => nameof(DemoScenario.Basic);

    public BasicFlowScenario(
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
            async (index, ct) =>
            {
                await RunOperationAsync(index, ct);
            });
    }

    private async Task RunOperationAsync(
        int index,
        CancellationToken cancellationToken)
    {
        var operationId = $"demo-runner-op-basic-{Guid.NewGuid()}";

        await _client.CreateOperationAsync(
            operationId,
            amount: "1000.00",
            currency: "RUB",
            description: $"basic-{index}",
            cancellationToken);

        await _client.SubmitOperationAsync(
            operationId,
            cancellationToken);

        await Task.Delay(
            _options.ReceiptProcessingWait,
            cancellationToken);

        await _client.GetReceiptsAsync(
            operationId,
            cancellationToken);

        await _client.GetOperationAsync(
            operationId,
            cancellationToken);

        await _client.GetEventsAsync(
            operationId,
            cancellationToken);

        await Task.Delay(
            _options.PauseBetweenOperations,
            cancellationToken);
    }
}