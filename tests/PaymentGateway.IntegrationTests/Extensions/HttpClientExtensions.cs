using System.Net.Http.Json;
using System.Text.Json;

internal static class HttpClientExtensions
{
    public static Task<HttpResponseMessage> CreateOperationAsync(
        this HttpClient client,
        string operationId,
        string amount,
        string currency,
        string description)
    {
        return client.PostAsJsonAsync(
            "/operations",
            new
            {
                operationId,
                amount,
                currency,
                description
            });
    }

    public static Task<HttpResponseMessage> SubmitOperationAsync(this HttpClient client, string operationId)
    {
        return client.PostAsync($"/operations/{operationId}/submit", content: null);
    }

    public static async Task<JsonElement> GetOperationAsync(this HttpClient client, string operationId)
    {
        var json = await client.GetFromJsonAsync<JsonElement>($"/operations/{operationId}");

        return json;
    }

    public static async Task<List<JsonElement>> GetEventsAsync(this HttpClient client, string operationId)
    {
        var list = await client.GetFromJsonAsync<List<JsonElement>>($"/operations/{operationId}/events");

        return list ?? [];
    }

    public static async Task<List<JsonElement>> GetReceiptsAsync(this HttpClient client, string operationId)
    {
        var list = await client.GetFromJsonAsync<List<JsonElement>>($"/operations/{operationId}/receipts");

        return list ?? [];
    }
}