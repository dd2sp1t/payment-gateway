namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class DuplicateCreateScenario : IScenario
{
    private readonly PaymentGatewayClient _client;

    public string Name => nameof(DemoScenario.Duplicate);

    public DuplicateCreateScenario(PaymentGatewayClient client)
    {
        _client = client;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var operationId = $"demo-runner-op-duplicate-{Guid.NewGuid()}";

        for (var i = 0; i < 10; i++)
        {
            await _client.CreateOperationAsync(
                operationId,
                amount: "1000.00",
                currency: "RUB",
                description: "duplicate",
                cancellationToken);
        }

        await _client.GetOperationAsync(
            operationId,
            cancellationToken);

        await _client.GetEventsAsync(
            operationId,
            cancellationToken);
    }
}