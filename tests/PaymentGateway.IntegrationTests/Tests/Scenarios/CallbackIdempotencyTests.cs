using PaymentGateway.Domain.Operations;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class CallbackIdempotencyTests : IntegrationTestBase
{
    public CallbackIdempotencyTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task DuplicateCompletedReceipt_ShouldBeSkipped()
    {
        // arrange
        var operationId = $"op-duplicate-completed-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        const int concurrentCount = 10;

        ScenarioStore
            .For(operationId)
            .SubmitAccepted();

        for (var i = 0; i < concurrentCount; i++)
        {
            ScenarioStore
                .For(operationId)
                .Callback(
                    result: ReceiptResult.Completed,
                    delay: TimeSpan.FromMilliseconds(500));
        }

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // act
        var callbackResponses = await Task.WhenAll(
            Enumerable.Range(0, concurrentCount)
                .Select(_ => ScenarioStore.DispatchNextCallbackAsync(operationId)));

        // assert
        foreach (var response in callbackResponses)
        {
            await Assert.AssertCallbackAcceptedAsync(response);
        }

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
    public async Task DuplicateRejectedReceipt_ShouldBeSkipped()
    {
        // arrange
        var operationId = $"op-duplicate-rejected-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        const int concurrentCount = 10;

        ScenarioStore
            .For(operationId)
            .SubmitAccepted();

        for (var i = 0; i < concurrentCount; i++)
        {
            ScenarioStore
                .For(operationId)
                .Callback(
                    result: ReceiptResult.Rejected,
                    delay: TimeSpan.FromMilliseconds(500));
        }

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // act
        var callbackResponses = await Task.WhenAll(
            Enumerable.Range(0, concurrentCount)
                .Select(_ => ScenarioStore.DispatchNextCallbackAsync(operationId)));

        // assert
        foreach (var response in callbackResponses)
        {
            await Assert.AssertCallbackAcceptedAsync(response);
        }

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