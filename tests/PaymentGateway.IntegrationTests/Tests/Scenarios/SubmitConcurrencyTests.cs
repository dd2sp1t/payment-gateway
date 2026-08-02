using System.Net;
using FluentAssertions;
using PaymentGateway.Domain.Operations;

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

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        const int concurrentCount = 10;

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        var createResponse = await Client.CreateOperationAsync(operationId, amount, currency, description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            expectedOperationId: operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

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

        foreach (var response in submitResponses)
        {
            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                await Assert.AssertSubmitScheduledAsync(response);
            }
            else
            {
                await Assert.AssertSubmitCurrentStateAsync(response);
            }
        }

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await Assert.AssertCallbackAcceptedAsync(callbackResponse);

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
}