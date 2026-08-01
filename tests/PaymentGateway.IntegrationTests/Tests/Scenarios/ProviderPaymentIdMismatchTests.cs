using System.Net;
using FluentAssertions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.Helpers;

namespace PaymentGateway.IntegrationTests.Tests.Scenarios;

public sealed class ProviderPaymentIdMismatchTests : IntegrationTestBase
{
    public ProviderPaymentIdMismatchTests(TestDatabaseFixture fixture) : base(fixture)
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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        // act
        var submitTask = Client.SubmitOperationAsync(operationId);

        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        await AssertHelper.AssertCallbackAcceptedAsync(callbackResponse);

        var submitResponse = await submitTask;

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // assert
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

        await AssertHelper.AssertOperationCreatedAsync(
            createResponse,
            operationId,
            expectedAmount: amount,
            expectedCurrency: currency,
            expectedDescription: description);

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // act
        var firstCallbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);
        var secondCallbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        firstCallbackResponse
            .Should()
            .NotBeNull();

        firstCallbackResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        await AssertHelper.AssertCallbackAcceptedAsync(secondCallbackResponse);

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED",
            "COMPLETED");
    }
}