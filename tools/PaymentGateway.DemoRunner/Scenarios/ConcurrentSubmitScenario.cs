using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class ConcurrentSubmitScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;
    public string Name => nameof(DemoScenario.Concurrent);

    public ConcurrentSubmitScenario(PaymentGatewayClient client, IOptions<DemoRunnerOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        var operationId = $"demo-runner-op-concurrent-{Guid.NewGuid()}";

        await _client.CreateAsync(
            operationId,
            amount: "1000.00",
            currency: "RUB",
            description: "concurrent",
            cancellationToken);

        var submits = Enumerable
            .Range(0, _options.ConcurrentSubmits)
            .Select(_ => _client.SubmitAsync(operationId, cancellationToken));

        await Task.WhenAll(submits);
    }
}