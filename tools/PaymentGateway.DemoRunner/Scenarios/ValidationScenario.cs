using Microsoft.Extensions.Options;

namespace PaymentGateway.DemoRunner.Scenarios;

internal sealed class ValidationScenario : IScenario
{
    private readonly PaymentGatewayClient _client;
    private readonly DemoRunnerOptions _options;

    public string Name => nameof(DemoScenario.Validation);

    public ValidationScenario(
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
                await RunValidationBatchAsync(ct);
            });
    }

    private Task RunValidationBatchAsync(CancellationToken cancellationToken)
    {
        var missingId = Guid.NewGuid().ToString();

        return Task.WhenAll(
            // CreateOperation validation

            _client.CreateOperationAsync(
                operationId: "",
                amount: "1000.00",
                currency: "RUB",
                description: "empty-id",
                cancellationToken),

            _client.CreateOperationAsync(
                operationId: Guid.NewGuid().ToString(),
                amount: "-100",
                currency: "RUB",
                description: "invalid-amount",
                cancellationToken),

            _client.CreateOperationAsync(
                operationId: Guid.NewGuid().ToString(),
                amount: "1000.00",
                currency: "USD",
                description: "invalid-currency",
                cancellationToken),

            _client.CreateOperationAsync(
                operationId: Guid.NewGuid().ToString(),
                amount: "1000.00",
                currency: "RUB",
                description: "",
                cancellationToken),

            // SubmitOperation validation

            _client.SubmitOperationAsync(
                operationId: "",
                cancellationToken),

            // ProcessReceipt validation

            _client.ProcessReceiptAsync(
                providerPaymentId: "",
                operationId: Guid.NewGuid().ToString(),
                result: "COMPLETED",
                message: "ok",
                occurredAt: DateTimeOffset.UtcNow,
                cancellationToken),

            _client.ProcessReceiptAsync(
                providerPaymentId: Guid.NewGuid().ToString(),
                operationId: "",
                result: "COMPLETED",
                message: "ok",
                occurredAt: DateTimeOffset.UtcNow,
                cancellationToken),

            _client.ProcessReceiptAsync(
                providerPaymentId: Guid.NewGuid().ToString(),
                operationId: Guid.NewGuid().ToString(),
                result: "",
                message: "ok",
                occurredAt: DateTimeOffset.UtcNow,
                cancellationToken),

            _client.ProcessReceiptAsync(
                providerPaymentId: Guid.NewGuid().ToString(),
                operationId: Guid.NewGuid().ToString(),
                result: "COMPLETED",
                message: "",
                occurredAt: DateTimeOffset.UtcNow,
                cancellationToken),

            _client.ProcessReceiptAsync(
                providerPaymentId: Guid.NewGuid().ToString(),
                operationId: Guid.NewGuid().ToString(),
                result: "COMPLETED",
                message: "ok",
                occurredAt: default,
                cancellationToken),

            // 404

            _client.SubmitOperationAsync(
                operationId: missingId,
                cancellationToken),

            _client.GetOperationAsync(
                operationId: missingId,
                cancellationToken),

            _client.GetEventsAsync(
                operationId: missingId,
                cancellationToken),

            _client.GetReceiptsAsync(
                operationId: missingId,
                cancellationToken)
        );
    }
}