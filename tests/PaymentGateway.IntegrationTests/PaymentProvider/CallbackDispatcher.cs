using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.PaymentProvider.Steps;

namespace PaymentGateway.IntegrationTests.PaymentProvider
{
    public sealed class CallbackDispatcher
    {
        private readonly ILogger<CallbackDispatcher> _logger;
        private readonly HttpClient _httpClient;

        public CallbackDispatcher(ILogger<CallbackDispatcher> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage?> DispatchAsync(string operationId, Guid providerPaymentId, Callback callback)
        {
            try
            {
                if (callback.Delay.HasValue)
                {
                    _logger.LogInformation(
                    "Callback delayed. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} DelayMs={DelayMs}",
                        operationId,
                        providerPaymentId,
                        callback.Delay.Value.TotalMilliseconds);

                    await Task.Delay(callback.Delay.Value);
                }

                _logger.LogInformation(
                    "Sending callback. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} Result={Result}",
                    operationId,
                    providerPaymentId,
                    callback.Result);

                var request = new
                {
                    providerPaymentId,
                    operationId,
                    result = callback.Result.ToString().ToUpperInvariant(),
                    message = callback.Result == ReceiptResult.Completed
                        ? "Payment completed"
                        : "Payment rejected",
                    occurredAt = DateTimeOffset.UtcNow
                };

                var requestBody = JsonSerializer.Serialize(request);

                _logger.LogDebug(
                    "Callback request. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} Body={Body}",
                    operationId,
                    providerPaymentId,
                    requestBody);

                var response = await _httpClient.PostAsJsonAsync("/receipts", request);

                var responseBody = await response.Content.ReadAsStringAsync();

                _logger.LogDebug(
                    "Callback response. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} StatusCode={StatusCode} Body={Body}",
                    operationId,
                    providerPaymentId,
                    response.StatusCode,
                    responseBody);

                return response;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Callback failed. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId}",
                    operationId,
                    providerPaymentId);

                return null;
            }
        }
    }
}