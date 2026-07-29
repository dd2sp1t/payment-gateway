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

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(ReceiptResult.Completed, delay: TimeSpan.FromMilliseconds(500))
            .Callback(ReceiptResult.Completed, delay: TimeSpan.FromMilliseconds(1000));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();
        (await Client.SubmitOperationAsync(operationId)).EnsureSuccessStatusCode();

        await ScenarioStore.DispatchNextCallbackAsync(operationId);
        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            "COMPLETED");

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

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(ReceiptResult.Rejected, delay: TimeSpan.FromMilliseconds(500))
            .Callback(ReceiptResult.Rejected, delay: TimeSpan.FromMilliseconds(1000));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();
        (await Client.SubmitOperationAsync(operationId)).EnsureSuccessStatusCode();

        await ScenarioStore.DispatchNextCallbackAsync(operationId);
        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            "REJECTED");

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

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(ReceiptResult.Completed, delay: TimeSpan.FromMilliseconds(500))
            .Callback(ReceiptResult.Rejected, delay: TimeSpan.FromMilliseconds(1000));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();
        (await Client.SubmitOperationAsync(operationId)).EnsureSuccessStatusCode();

        await ScenarioStore.DispatchNextCallbackAsync(operationId);
        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        await AssertHelper.AssertStatusIsStable(
            Client,
            operationId,
            "COMPLETED");

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED",
            "IGNORED");
    }
}