using System.Net.Http.Json;

namespace PaymentGateway.DemoRunner;

internal sealed class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public PaymentGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateAndSubmitAsync(
        string operationId,
        string amount,
        string currency,
        string description,
        CancellationToken cancellationToken)
    {
        await CreateAsync(operationId, amount, currency, description, cancellationToken);
        await SubmitAsync(operationId, cancellationToken);
    }

    public Task CreateAsync(
        string operationId,
        string amount,
        string currency,
        string description,
        CancellationToken cancellationToken)
    {
        return _httpClient.PostAsJsonAsync(
            "/operations",
            new
            {
                operationId,
                amount,
                currency,
                description
            },
            cancellationToken);
    }

    public Task SubmitAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        return _httpClient.PostAsync(
            $"/operations/{operationId}/submit",
            content: null,
            cancellationToken);
    }
}