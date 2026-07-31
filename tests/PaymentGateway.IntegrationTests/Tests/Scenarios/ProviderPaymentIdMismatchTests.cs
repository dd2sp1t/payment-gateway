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
    public async Task Submit_WhenProviderReturnsDifferentPaymentId_ShouldNotCompleteDispatch()
    {
        // arrange
        var operationId = $"op-provider-mismatch-submit-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        var callbackPaymentId = Guid.NewGuid();
        var submitPaymentId = Guid.NewGuid();

        submitPaymentId
            .Should()
            .NotBe(callbackPaymentId);

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(
                providerPaymentId: submitPaymentId,
                delay: TimeSpan.FromMilliseconds(5000))
            .SubmitAccepted(
                providerPaymentId: submitPaymentId)
            .Callback(
                result: ReceiptResult.Completed,
                providerPaymentId: callbackPaymentId,
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
    public async Task Callback_WithDifferentProviderPaymentId_ShouldReturnConflict()
    {
        // arrange
        var operationId = $"op-provider-mismatch-callback-{Guid.NewGuid()}";

        const string amount = "1000.00";
        const string currency = "RUB";
        const string description = "integration_test";

        var submitPaymentId = Guid.NewGuid();
        var callbackPaymentId = Guid.NewGuid();

        submitPaymentId
            .Should()
            .NotBe(callbackPaymentId);

        ScenarioStore
            .For(operationId)
            .SubmitAccepted(providerPaymentId: submitPaymentId)
            .Callback(
                result: ReceiptResult.Completed,
                providerPaymentId: callbackPaymentId,
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

        var submitResponse = await Client.SubmitOperationAsync(operationId);

        await AssertHelper.AssertSubmitScheduledAsync(submitResponse);

        // act
        var callbackResponse = await ScenarioStore.DispatchNextCallbackAsync(operationId);

        // assert
        callbackResponse
            .Should()
            .NotBeNull();

        callbackResponse.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        var events = await Client.GetEventsAsync(operationId);

        AssertHelper.AssertEventSequence(
            events,
            "CREATED",
            "SUBMITTED");
    }
}