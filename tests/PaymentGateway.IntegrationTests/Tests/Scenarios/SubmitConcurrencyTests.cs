using System.Net;
using FluentAssertions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class SubmitConcurrencyTests : IntegrationTestBase
{
    public SubmitConcurrencyTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Submit_MultipleConcurrentRequests_ShouldDispatchExactlyOnce()
    {
        // arrange
        var operationId = $"op-concurrent-{Guid.NewGuid()}";

        const int concurrentCount = 10;

        for (var i = 0; i < concurrentCount; i++)
        {
            ScenarioStore
                .For(operationId)
                .SubmitAccepted();
        }

        ScenarioStore
            .For(operationId)
            .Callback(
                ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        (await Client.CreateOperationAsync(operationId))
            .EnsureSuccessStatusCode();

        // act
        var submitResponses = await Task.WhenAll(
            Enumerable
                .Range(0, concurrentCount)
                .Select(_ => Client.SubmitOperationAsync(operationId)));

        // assert
        submitResponses
            .Should()
            .ContainSingle(r => r.StatusCode == HttpStatusCode.Accepted);

        submitResponses
            .Should()
            .OnlyContain(r =>
                r.StatusCode == HttpStatusCode.Accepted ||
                r.StatusCode == HttpStatusCode.OK);

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

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
}