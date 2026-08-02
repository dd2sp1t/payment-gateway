namespace PaymentGateway.DemoRunner.Scenarios;

internal interface IScenario
{
    string Name { get; }

    Task RunAsync(CancellationToken cancellationToken);
}