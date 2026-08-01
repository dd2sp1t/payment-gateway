using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.PaymentProvider;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public sealed class DispatchRetryTests : IntegrationTestBase
{
    public DispatchRetryTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    public static IEnumerable<object[]> RetryScenarios =>
    [
        ["503", (Action<PaymentProviderScenarioBuilder>)(b => b.ServiceUnavailable())],
        ["504", (Action<PaymentProviderScenarioBuilder>)(b => b.GatewayTimeout())],
        ["429", (Action<PaymentProviderScenarioBuilder>)(b => b.TooManyRequests())],
        ["timeout", (Action<PaymentProviderScenarioBuilder>)(b => b.Timeout())],
        ["socket", (Action<PaymentProviderScenarioBuilder>)(b => b.SocketError())],
        ["io", (Action<PaymentProviderScenarioBuilder>)(b => b.IoError())]
    ];

    [Theory]
    [MemberData(nameof(RetryScenarios))]
    public async Task Operation_ShouldRetry_WhenTransientFailureOccurs(
        string scenarioName,
        Action<PaymentProviderScenarioBuilder> configure)
    {
        var operationId = $"op-{scenarioName}-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        var builder = ScenarioStore.For(operationId);

        configure(builder);

        builder
            .SubmitAccepted()
            .Callback(
                ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        var createResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        await Assert.AssertRetryScheduledAsync(
            Client,
            operationId,
            expectedRetryCount: 1);

        var callback = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callback);

        await Assert.AssertOperationStatusIsStableAsync(
            Client,
            operationId,
            "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        await Assert.AssertEventSequenceAsync(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED");
    }

    [Fact]
    public async Task Operation_ShouldStopRetrying_WhenRetryLimitReached()
    {
        // arrange
        var operationId = $"op-retry-limit-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        for (var i = 0; i <= TestOptions.DispatchMaxRetryCount; i++)
        {
            ScenarioStore
                .For(operationId)
                .ServiceUnavailable();
        }

        var createResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // assert
        await Assert.AssertOperationStatusAsync(
            Client,
            operationId,
            expectedStatus: "PROCESSING");

        await Assert.AssertRetryStoppedAsync(
            Client,
            operationId,
            expectedRetryCount: TestOptions.DispatchMaxRetryCount);
    }
}