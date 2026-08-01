using PaymentGateway.Domain.Operations;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class EarlyCallbackTests : IntegrationTestBase
{
    public EarlyCallbackTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task CompletedReceipt_BeforeProviderResponse_ShouldCompleteOperation()
    {
        // arrange
        var operationId = $"op-early-completed-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(
                delay: TimeSpan.FromMilliseconds(5000))
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(1000));

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        // act
        var submitTask = Client.SubmitOperationAsync(operationId);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callbackResponse);

        var submitResponse = await submitTask;

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // assert
        await Assert.AssertOperationStatusIsStableAsync(
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
    public async Task RejectedReceipt_BeforeProviderResponse_ShouldRejectOperation()
    {
        // arrange
        var operationId = $"op-early-rejected-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(
                delay: TimeSpan.FromMilliseconds(5000))
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(1000));

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        // act
        var submitTask = Client.SubmitOperationAsync(operationId);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callbackResponse);


        var submitResponse = await submitTask;

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // assert
        await Assert.AssertOperationStatusIsStableAsync(
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