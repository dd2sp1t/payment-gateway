using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace PaymentGateway.IntegrationTests.Helpers;

internal static class AssertHelper
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan StabilityDelay = TimeSpan.FromSeconds(6);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task AssertOperationCreatedAsync(
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

    public static async Task AssertSubmitScheduledAsync(HttpResponseMessage response)
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

    public static async Task AssertSubmitCurrentStateAsync(HttpResponseMessage response)
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

    public static async Task AssertCallbackAcceptedAsync(HttpResponseMessage? response)
    {
        response
            .Should()
            .NotBeNull();

        response.StatusCode
            .Should()
            .Be(HttpStatusCode.NoContent);

        var body = await response.Content.ReadAsStringAsync();

        body
            .Should()
            .BeEmpty();
    }

    public static async Task AssertStatusIsStable(
        HttpClient client,
        string operationId,
        string expectedStatus)
    {
        await Task.Delay(InitialDelay);

        var operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be(expectedStatus);

        await Task.Delay(StabilityDelay);

        operation = await client.GetOperationAsync(operationId);

        operation
            .GetProperty("status")
            .GetString()
            .Should()
            .Be(expectedStatus);
    }

    public static void AssertEventSequence(IReadOnlyCollection<JsonElement> events, params string[] expected)
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
    }

    public static async Task AssertOperationHasSingleTerminalEventAndIgnoredAsync(HttpClient client, string operationId)
    {
        await Task.Delay(InitialDelay);

        var operation = await client.GetOperationAsync(operationId);

        var status = operation
            .GetProperty("status")
            .GetString();

        status
            .Should()
            .BeOneOf("COMPLETED", "REJECTED");

        await Task.Delay(StabilityDelay);

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
}