using FluentAssertions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class BasicFlowTests : IntegrationTestBase
{
    public BasicFlowTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Operation_ShouldGoToCompleted_WhenProviderAcceptsAndSendsCompletedCallback()
    {
        // arrange
        var operationId = $"op-basic-success-{Guid.NewGuid()}";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();
        (await Client.SubmitOperationAsync(operationId)).EnsureSuccessStatusCode();

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Task.Delay(1000);

        var operation = await Client.GetOperationAsync(operationId);
        var events = await Client.GetEventsAsync(operationId);

        // assert
        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("COMPLETED");

        AssertHelper.AssertEventSequence(
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

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(500));

        // act
        (await Client.CreateOperationAsync(operationId)).EnsureSuccessStatusCode();
        (await Client.SubmitOperationAsync(operationId)).EnsureSuccessStatusCode();

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Task.Delay(1000);

        var operation = await Client.GetOperationAsync(operationId);
        var events = await Client.GetEventsAsync(operationId);

        // assert
        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("REJECTED");

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "REJECTED");
    }
}