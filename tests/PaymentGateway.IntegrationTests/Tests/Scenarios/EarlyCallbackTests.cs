using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class EarlyCallbackTests : IntegrationTestBase
{
    public EarlyCallbackTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task CompletedReceipt_BeforeProviderResponse_ShouldCompleteOperation()
    {
        // arrange
        var operationId = $"op-early-completed-{Guid.NewGuid()}";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(delay: TimeSpan.FromMilliseconds(5000))
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(1000));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();

        var submitTask = Client.SubmitOperationAsync(operationId);

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        (await submitTask).EnsureSuccessStatusCode();

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
    public async Task RejectedReceipt_BeforeProviderResponse_ShouldRejectOperation()
    {
        // arrange
        var operationId = $"op-early-rejected-{Guid.NewGuid()}";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(delay: TimeSpan.FromMilliseconds(5000))
            .Callback(
                result: ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(1000));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();

        var submitTask = Client.SubmitOperationAsync(operationId);

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        (await submitTask).EnsureSuccessStatusCode();

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
}