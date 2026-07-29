using System.Text.Json;
using FluentAssertions;

namespace PaymentGateway.IntegrationTests.Helpers;

internal static class AssertHelper
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan StabilityDelay = TimeSpan.FromSeconds(6);

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
        events.Should().NotBeNullOrEmpty();

        events.Should().HaveCount(expected.Length);

        events
            .Select(e => e.GetProperty("type").GetString())
            .Should()
            .ContainInOrder(expected);
    }
}