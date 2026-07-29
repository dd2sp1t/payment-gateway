using System.Net;
using FluentAssertions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class SubmitConcurrencyTests : IntegrationTestBase
{
    public SubmitConcurrencyTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task Submit_MultipleConcurrentRequests_ShouldDispatchExactlyOnce()
    {
        // arrange
        var operationId = $"op-concurrent-{Guid.NewGuid()}";

        const int concurrentCount = 10;
        for (int i = 0; i < concurrentCount; i++)
        {
            ScenarioStore
                .For(operationId)
                .SubmitAccepted();
        }

        ScenarioStore
            .For(operationId)
            .Callback(ReceiptResult.Completed, delay: TimeSpan.FromMilliseconds(500));

        var createResponse = await Client.CreateOperationAsync(operationId);
        createResponse.EnsureSuccessStatusCode();

        var tasks = new List<Task<HttpResponseMessage>>();

        // act
        for (int i = 0; i < concurrentCount; i++)
        {
            tasks.Add(Client.SubmitOperationAsync(operationId));
        }

        var submitResponses = await Task.WhenAll(tasks);

        // assert
        submitResponses
            .Should()
            .ContainSingle(r => r.StatusCode == HttpStatusCode.Accepted);

        submitResponses
            .Should()
            .OnlyContain(r =>
                r.StatusCode == HttpStatusCode.Accepted ||
                r.StatusCode == HttpStatusCode.OK);

        await Task.Delay(5000);

        await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Task.Delay(1000);

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED");
    }
}