using System.Net.Http.Json;

namespace PaymentGateway.DemoRunner;

internal sealed class PaymentGatewayClient
{
    private readonly HttpClient _httpClient;

    public PaymentGatewayClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task CreateOperationAndSubmitAsync(
        string operationId,
        string amount,
        string currency,
        string description,
        CancellationToken cancellationToken)
    {
        await CreateOperationAsync(operationId, amount, currency, description, cancellationToken);
        await SubmitOperationAsync(operationId, cancellationToken);
    }

    public Task CreateOperationAsync(
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

    public Task SubmitOperationAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        return _httpClient.PostAsync(
            $"/operations/{operationId}/submit",
            content: null,
            cancellationToken);
    }

    public Task ProcessReceiptAsync(
        string providerPaymentId,
        string operationId,
        string result,
        string message,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        return _httpClient.PostAsJsonAsync(
            "/receipts",
            new
            {
                providerPaymentId,
                operationId,
                result,
                message,
                occurredAt
            },
            cancellationToken);
    }

    public Task GetOperationAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        return _httpClient.GetAsync(
            $"/operations/{operationId}",
            cancellationToken);
    }

    public Task GetEventsAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        return _httpClient.GetAsync(
            $"/operations/{operationId}/events",
            cancellationToken);
    }

    public Task GetReceiptsAsync(
        string operationId,
        CancellationToken cancellationToken)
    {
        return _httpClient.GetAsync(
            $"/operations/{operationId}/receipts",
            cancellationToken);
    }
}