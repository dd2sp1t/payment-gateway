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
}