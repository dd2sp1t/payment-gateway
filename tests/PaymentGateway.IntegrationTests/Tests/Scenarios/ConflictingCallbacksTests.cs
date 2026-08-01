using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public class ConflictingCallbacksTests : IntegrationTestBase
{
    public ConflictingCallbacksTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task ConcurrentCompletedAndRejectedReceipts_ShouldPersistIgnoredReceipt()
    {
        // arrange
        var operationId = $"op-concurrent-opposite-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted();

        ScenarioStore
            .For(operationId)
            .Callback(
                ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(500));

        ScenarioStore
            .For(operationId)
            .Callback(
                ReceiptResult.Rejected,
                delay: TimeSpan.FromMilliseconds(500));

        var createResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

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
            ScenarioStore.DispatchNextCallbackAsync(operationId),
            ScenarioStore.DispatchNextCallbackAsync(operationId));

        // assert
        foreach (var response in callbackResponses)
        {
            await AssertHelper.AssertCallbackAcceptedAsync(response);
        }

        await AssertHelper.AssertOperationHasSingleTerminalEventAndIgnoredAsync(Client, operationId);
    }
}