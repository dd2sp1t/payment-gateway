using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await AssertHelper.AssertCallbackAcceptedAsync(callbackResponse);

        // assert
        await AssertHelper.AssertOperationStatusAsync(
            Client,
            operationId,
            expectedStatus: "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        await AssertHelper.AssertEventSequenceAsync(
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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await AssertHelper.AssertCallbackAcceptedAsync(callbackResponse);

        // assert
        await AssertHelper.AssertOperationStatusAsync(
            Client,
            operationId,
            expectedStatus: "REJECTED");

        var events = await Client.GetEventsAsync(operationId);

        await AssertHelper.AssertEventSequenceAsync(
            events,
            "CREATED",
            "SUBMITTED",
            "REJECTED");
    }
}