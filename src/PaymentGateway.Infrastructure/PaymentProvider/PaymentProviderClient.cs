using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Infrastructure.PaymentProvider;

internal sealed class PaymentProviderClient : IPaymentProviderClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentProviderClient> _logger;

    public PaymentProviderClient(HttpClient httpClient, ILogger<PaymentProviderClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SubmitPaymentResponse> SubmitAsync(
        SubmitPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var requestBody = JsonSerializer.Serialize(request, JsonOptions);

        _logger.LogInformation(
            "Provider request. OperationId={OperationId}. Body={Body}.",
            request.OperationId,
            requestBody);

        using var requestMessage = CreateRequestMessage(request);

        HttpResponseMessage responseMessage;

        try
        {
            responseMessage = await _httpClient.SendAsync(requestMessage, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Provider request failed. OperationId={OperationId}.",
                request.OperationId);

            throw new PaymentProviderException("Provider request failed.", exception);
        }

        var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        EnsureSuccessStatusCode(
            responseMessage,
            request.OperationId,
            responseBody);

        _logger.LogInformation(
            "Provider response. OperationId={OperationId}. StatusCode={StatusCode}. Body={Body}.",
            request.OperationId,
            (int)responseMessage.StatusCode,
            responseBody);

        var response = await DeserializeResponseAsync(
            responseMessage.Content,
            request.OperationId,
            responseBody,
            cancellationToken);

        return response;
    }

    private static HttpRequestMessage CreateRequestMessage(SubmitPaymentRequest request)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, "payments");

        requestMessage.Headers.Add("Idempotency-Key", request.OperationId);
        requestMessage.Headers.Add("X-Correlation-ID", request.OperationId);

        requestMessage.Content = JsonContent.Create(request, options: JsonOptions);

        return requestMessage;
    }

    private void EnsureSuccessStatusCode(
        HttpResponseMessage responseMessage,
        string operationId,
        string responseBody)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            return;
        }

        _logger.LogWarning(
            "Provider returned unsuccessful status. OperationId={OperationId}. StatusCode={StatusCode}. Body={Body}.",
            operationId,
            (int)responseMessage.StatusCode,
            responseBody);

        throw new PaymentProviderException($"Provider returned HTTP {(int)responseMessage.StatusCode}.");
    }

    private async Task<SubmitPaymentResponse> DeserializeResponseAsync(
        HttpContent content,
        string operationId,
        string responseBody,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await content.ReadFromJsonAsync<SubmitPaymentResponse>(
                JsonOptions,
                cancellationToken);

            if (response is not null)
            {
                return response;
            }

            _logger.LogWarning("Provider returned empty response. OperationId={OperationId}.", operationId);

            throw new PaymentProviderException("Provider returned empty response.");
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Provider response deserialization failed. OperationId={OperationId}. Body={Body}.",
                operationId,
                responseBody);

            throw new PaymentProviderException("Failed to deserialize provider response.", exception);
        }
    }
}