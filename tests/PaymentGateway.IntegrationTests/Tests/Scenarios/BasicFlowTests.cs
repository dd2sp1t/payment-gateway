using PaymentGateway.Domain.Operations;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class BasicFlowTests : IntegrationTestBase
{
    public BasicFlowTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Operation_ShouldGoToCompleted_WhenProviderAcceptsAndSendsCompletedCallback()
    {
        // arrange
        var operationId = $"op-basic-success-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        // act
        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callbackResponse);

        // assert
        await Assert.AssertOperationStatusAsync(
            Client,
            operationId,
            expectedStatus: "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        await Assert.AssertEventSequenceAsync(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED");
    }

    [Fact]
    public async Task Operation_ShouldGoToRejected_WhenProviderAcceptsButSendsRejectedCallback()
    {
        // arrange
        var operationId = $"op-basic-rejected-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(500));

        // act
        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callbackResponse);

        // assert
        await Assert.AssertOperationStatusAsync(
            Client,
            operationId,
            expectedStatus: "REJECTED");

        var events = await Client.GetEventsAsync(operationId);

        await Assert.AssertEventSequenceAsync(
            events,
            "CREATED",
            "SUBMITTED",
            "REJECTED");
    }
}