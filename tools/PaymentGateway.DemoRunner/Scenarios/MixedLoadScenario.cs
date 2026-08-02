using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class MixedLoadScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;
    private readonly Random _random = new();
    public string Name => nameof(DemoScenario.Mixed);

    public MixedLoadScenario(PaymentGatewayClient client, IOptions<DemoRunnerOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        for (var i = 0; i < _options.Operations; i++)
        {
            var operationId = $"demo-runner-op-mixed-{Guid.NewGuid()}";

            if (_random.Next(100) < 80)
            {
                await _client.CreateAndSubmitAsync(
                    operationId,
                    amount: "1000.00",
                    currency: "RUB",
                    description: $"mixed-{i}",
                    cancellationToken);
            }
            else
            {
                await _client.CreateAsync(
                    operationId,
                    amount: "1000.00",
                    currency: "RUB",
                    description: $"mixed-{i}",
                    cancellationToken);

                var submits = Enumerable
                    .Range(0, _options.ConcurrentSubmits)
                    .Select(_ => _client.SubmitAsync(operationId, cancellationToken));

                await Task.WhenAll(submits);
            }

            await Task.Delay(_options.PauseBetweenOperations, cancellationToken);
        }
    }
}