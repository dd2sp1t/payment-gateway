using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients;

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
        int retryCount,
        CancellationToken cancellationToken)
    {
        var requestBody = JsonSerializer.Serialize(request, JsonOptions);

        _logger.LogInformation(
            "Provider request. OperationId={OperationId} RetryCount={RetryCount}",
            request.OperationId,
            retryCount);

        _logger.LogDebug(
            "Provider request. OperationId={OperationId} RetryCount={RetryCount} Body={Body}",
            request.OperationId,
            retryCount,
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
                "Provider request failed. OperationId={OperationId} RetryCount={RetryCount} Body={Body}",
                request.OperationId,
                retryCount,
                requestBody);

            throw new PaymentProviderException("Provider request failed.", exception);
        }

        var responseBody = await responseMessage.Content.ReadAsStringAsync(cancellationToken);

        EnsureSuccessStatusCode(
            responseMessage,
            request.OperationId,
            retryCount,
            responseBody);

        _logger.LogInformation(
            "Provider response. OperationId={OperationId} RetryCount={RetryCount} StatusCode={StatusCode}",
            request.OperationId,
            retryCount,
            (int)responseMessage.StatusCode);

        _logger.LogDebug(
            "Provider response. OperationId={OperationId} RetryCount={RetryCount} StatusCode={StatusCode} Body={Body}",
            request.OperationId,
            retryCount,
            (int)responseMessage.StatusCode,
            responseBody);

        var response = await DeserializeResponseAsync(
            responseMessage.Content,
            request.OperationId,
            retryCount,
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
        int retryAttempt,
        string responseBody)
    {
        if (responseMessage.IsSuccessStatusCode)
        {
            return;
        }

        _logger.LogWarning(
            "Provider returned unsuccessful status. OperationId={OperationId} RetryCount={RetryCount} StatusCode={StatusCode} Body={Body}",
            operationId,
            retryAttempt,
            (int)responseMessage.StatusCode,
            responseBody);

        throw new PaymentProviderException($"Provider returned HTTP {(int)responseMessage.StatusCode}.");
    }

    private async Task<SubmitPaymentResponse> DeserializeResponseAsync(
        HttpContent content,
        string operationId,
        int retryAttempt,
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

            _logger.LogWarning(
                "Provider returned empty response. OperationId={OperationId} RetryCount={RetryCount}",
                operationId,
                retryAttempt);

            throw new PaymentProviderException("Provider returned empty response.");
        }
        catch (JsonException exception)
        {
            _logger.LogError(
                exception,
                "Provider response deserialization failed. OperationId={OperationId} RetryCount={RetryCount} Body={Body}",
                operationId,
                retryAttempt,
                responseBody);

            throw new PaymentProviderException("Failed to deserialize provider response.", exception);
        }
    }
}