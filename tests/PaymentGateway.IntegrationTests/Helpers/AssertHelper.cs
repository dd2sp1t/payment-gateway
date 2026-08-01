using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using PaymentGateway.Application.BackgroundServices.DispatchOperations;

namespace PaymentGateway.IntegrationTests.Helpers;

public class AssertHelper
{
    private readonly TimeSpan _dispatchInterval;
    private readonly TimeSpan _stabilityDelay;
    private readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AssertHelper(
        DispatchOperationsBackgroundServiceOptions options,
        TimeSpan stabilityDelay)
    {
        _dispatchInterval = options.Interval;
        _stabilityDelay = stabilityDelay;
    }

    public async Task AssertOperationCreatedAsync(
        HttpResponseMessage response,
        string expectedOperationId,
        string expectedAmount,
        string expectedCurrency,
        string expectedDescription)
    {
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Created);

        var operation = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        operation
            .GetProperty("operationId")
            .GetString()
            .Should()
            .Be(expectedOperationId);

        operation
            .GetProperty("amount")
            .GetString()
            .Should()
            .Be(expectedAmount);

        operation
            .GetProperty("currency")
            .GetString()
            .Should()
            .Be(expectedCurrency);

        operation
            .GetProperty("description")
            .GetString()
            .Should()
            .Be(expectedDescription);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("CREATED");
    }

    public async Task AssertSubmitScheduledAsync(HttpResponseMessage response)
    {
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Accepted);

        var submit = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        submit
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("PROCESSING");

        submit
            .GetProperty("operationId")
            .GetString()
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    public async Task AssertSubmitCurrentStateAsync(HttpResponseMessage response)
    {
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.OK);

        var submit = await response.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);

        submit
            .GetProperty("status")
            .GetString()
            .Should()
            .BeOneOf(
                "PROCESSING",
                "COMPLETED",
                "REJECTED");

        submit
            .GetProperty("operationId")
            .GetString()
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    public async Task AssertCallbackAcceptedAsync(HttpResponseMessage response)
    {
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        var body = await response.Content.ReadAsStringAsync();

        body
            .Should()
            .BeEmpty();
    }

    public async Task AssertOperationStatusAsync(
        HttpClient client,
        string operationId,
        string expectedStatus)
    {
        var operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be(expectedStatus);
    }

    public async Task AssertOperationStatusIsStableAsync(
        HttpClient client,
        string operationId,
        string expectedStatus)
    {
        await AssertOperationStatusAsync(client, operationId, expectedStatus);

        await Task.Delay(_stabilityDelay);

        await AssertOperationStatusAsync(client, operationId, expectedStatus);
    }

    public Task AssertEventSequenceAsync(IReadOnlyCollection<JsonElement> events, params string[] expected)
    {
        events
            .Should()
            .NotBeNullOrEmpty();

        events
            .Should()
            .HaveCount(expected.Length);

        events
            .Select(e => e.GetProperty("type").GetString())
            .Should()
            .ContainInOrder(expected);

        return Task.CompletedTask;
    }

    public async Task AssertOperationHasSingleTerminalEventAndIgnoredAsync(HttpClient client, string operationId)
    {
        var operation = await client.GetOperationAsync(operationId);

        var status = operation
            .GetProperty("status")
            .GetString();

        status
            .Should()
            .BeOneOf("COMPLETED", "REJECTED");

        await Task.Delay(_stabilityDelay);

        operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be(status);

        var events = await client.GetEventsAsync(operationId);

        events
            .Should()
            .HaveCount(4);

        events[0]
            .GetProperty("type")
            .GetString()
            .Should()
            .Be("CREATED");

        events[1]
            .GetProperty("type")
            .GetString()
            .Should()
            .Be("SUBMITTED");

        events[2]
            .GetProperty("type")
            .GetString()
            .Should()
            .Be(status);

        events[3]
            .GetProperty("type")
            .GetString()
            .Should()
            .Be("IGNORED");

        var receipts = await client.GetReceiptsAsync(operationId);

        receipts
            .Should()
            .HaveCount(2);

        receipts
            .Select(x => x.GetProperty("result").GetString())
            .Should()
            .BeEquivalentTo(
            [
            "COMPLETED",
            "REJECTED"
            ]);
    }

    public Task AssertConflictAsync(HttpResponseMessage response)
    {
        response.StatusCode
            .Should()
            .Be(HttpStatusCode.Conflict);

        return Task.CompletedTask;
    }

    public async Task AssertRetryScheduledAsync(
        HttpClient client,
        string operationId,
        int expectedRetryCount)
    {
        await WaitForRetryAsync(expectedRetryCount);

        var operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be("PROCESSING");

        operation
            .GetProperty("retryCount")
            .GetInt32()
            .Should()
            .Be(expectedRetryCount);

        operation
            .GetProperty("nextDispatchAt")
            .GetString()
            .Should()
            .NotBeNullOrWhiteSpace();
    }

    public async Task AssertRetryStoppedAsync(
        HttpClient client,
        string operationId,
        int expectedRetryCount)
    {
        await WaitForRetryAsync(expectedRetryCount);

        var operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("retryCount")
            .GetInt32()
            .Should()
            .Be(expectedRetryCount);

        operation
            .GetProperty("nextDispatchAt")
            .ValueKind
            .Should()
            .Be(JsonValueKind.Null);
    }

    private Task WaitForRetryAsync(int retryNumber)
    {
        var delay = TimeSpan.FromMilliseconds(_dispatchInterval.TotalMilliseconds * (retryNumber + 1));

        return Task.Delay(delay);
    }
}