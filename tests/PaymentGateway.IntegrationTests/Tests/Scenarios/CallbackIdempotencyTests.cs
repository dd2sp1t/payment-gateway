using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // act
        var callbackResponses = await Task.WhenAll(
            Enumerable.Range(0, concurrentCount)
                .Select(_ => ScenarioStore.DispatchNextCallbackAsync(operationId)));

        // assert
        foreach (var response in callbackResponses)
        {
            await AssertHelper.AssertCallbackAcceptedAsync(response);
        }

        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            expectedStatus: "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // act
        var callbackResponses = await Task.WhenAll(
            Enumerable.Range(0, concurrentCount)
                .Select(_ => ScenarioStore.DispatchNextCallbackAsync(operationId)));

        // assert
        foreach (var response in callbackResponses)
        {
            await AssertHelper.AssertCallbackAcceptedAsync(response);
        }

        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            expectedStatus: "REJECTED");

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "REJECTED");
    }

    [Fact]
    public async Task CompletedReceipt_FollowedByRejectedReceipt_ShouldCreateIgnoredEvent()
    {
        // arrange
        var operationId = $"op-late-opposite-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500))
            .Callback(
                result: ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(1000));

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // act
        var firstCallback = await ScenarioStore.DispatchNextCallbackAsync(operationId);
        var secondCallback = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        await AssertHelper.AssertCallbackAcceptedAsync(firstCallback);
        await AssertHelper.AssertCallbackAcceptedAsync(secondCallback);

        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            expectedStatus: "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED",
            "IGNORED");
    }
}