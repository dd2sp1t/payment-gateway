using System.Text.Json;
using FluentAssertions;

namespace PaymentGateway.IntegrationTests.Helpers;

internal static class AssertHelper
{
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