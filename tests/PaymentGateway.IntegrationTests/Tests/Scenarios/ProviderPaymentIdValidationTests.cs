using PaymentGateway.Domain.Operations;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public sealed class ProviderPaymentIdValidationTests : IntegrationTestBase
{
    public ProviderPaymentIdValidationTests(TestDatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Submit_ShouldCompleteAfterRetry_WhenProviderPaymentIdsDiffer()
    {
        // arrange
        var operationId = $"op-provider-mismatch-submit-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(
                providerPaymentId: Guid.NewGuid(),
                delay: TimeSpan.FromMilliseconds(5000))
            .Callback(
                result: ReceiptResult.Completed,
                delay: TimeSpan.FromMilliseconds(1000));

        var createResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            operationId,
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
    public async Task Callback_ShouldReturnConflict_WhenProviderPaymentIdsDiffer()
    {
        // arrange
        var operationId = $"op-provider-mismatch-callback-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        ScenarioStore
            .For(operationId)
            .SubmitAccepted()
            .Callback(
                result: ReceiptResult.Completed,
                providerPaymentId: Guid.NewGuid(),
                delay: TimeSpan.FromMilliseconds(1000))
            .Callback(
                result: ReceiptResult.Completed);

        var createResponse = await Client.CreateOperationAsync(
            operationId,
            amount,
            currency,
            description);

        await Assert.AssertOperationCreatedAsync(
            createResponse,
            operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await Assert.AssertSubmitScheduledAsync(submitResponse);

        // act
        var firstCallbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);
        var secondCallbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        await Assert.AssertConflictAsync(firstCallbackResponse);
        await Assert.AssertCallbackAcceptedAsync(secondCallbackResponse);

        var events = await Client.GetEventsAsync(operationId);

        await Assert.AssertEventSequenceAsync(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED");
    }
}