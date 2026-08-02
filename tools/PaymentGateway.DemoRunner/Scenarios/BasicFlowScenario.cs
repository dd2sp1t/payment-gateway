using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class BasicFlowScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;
    public string Name => nameof(DemoScenario.Basic);

    public BasicFlowScenario(PaymentGatewayClient client, IOptions<DemoRunnerOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _options.Operations; i++)
        {
            var operationId = $"demo-runner-op-basic-{Guid.NewGuid()}";

            await _client.CreateAndSubmitAsync(
                operationId,
                amount: "1000.00",
                currency: "RUB",
                description: $"basic-{i}",
                cancellationToken);

            await Task.Delay(_options.PauseBetweenOperations, cancellationToken);
        }
    }
}