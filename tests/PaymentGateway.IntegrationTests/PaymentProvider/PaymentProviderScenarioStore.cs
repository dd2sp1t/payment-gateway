using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using PaymentGateway.IntegrationTests.PaymentProvider.Steps;

namespace PaymentGateway.IntegrationTests.PaymentProvider;

public sealed class PaymentProviderScenarioStore
{
    private readonly ConcurrentDictionary<string, PaymentProviderScenarioBuilder> _builders = new();
    private readonly ConcurrentDictionary<string, Guid> _paymentIds = new();
    private readonly ILogger<PaymentProviderScenarioStore> _logger;
    private readonly CallbackDispatcher _callbackDispatcher;

    public PaymentProviderScenarioStore(
        ILogger<PaymentProviderScenarioStore> logger,
        CallbackDispatcher callbackDispatcher)
    {
        _logger = logger;
        _callbackDispatcher = callbackDispatcher;
    }

    public PaymentProviderScenarioBuilder For(string operationId)
    {
        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode}",
            operationId,
            nameof(For),
            GetHashCode());

        return _builders.GetOrAdd(operationId, _ => new());
    }

    internal PaymentProviderScenarioBuilder GetBuilder(string operationId)
    {
        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode}",
            operationId,
            nameof(GetBuilder),
            GetHashCode());

        return _builders.GetOrAdd(operationId, _ => new());
    }

    internal Submit? NextSubmit(string operationId)
    {
        var builder = GetBuilder(operationId);

        var submit = builder.NextSubmit();

        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode} Submit={Submit}",
            operationId,
            nameof(NextSubmit),
            GetHashCode(),
            submit);

        return submit;
    }

    internal Callback? NextCallback(string operationId)
    {
        var builder = GetBuilder(operationId);

        var callback = builder.NextCallback();

        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode} Callback={Callback}",
            operationId,
            nameof(DispatchNextCallbackAsync),
            GetHashCode(),
            callback);

        return callback;
    }

    internal Guid GetProviderPaymentId(string operationId)
    {
        var id = _paymentIds.GetOrAdd(operationId, _ => DeterministicGuid(operationId));

        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode} ProviderPaymentId={ProviderPaymentId}",
            operationId,
            nameof(GetProviderPaymentId),
            GetHashCode(),
            id);

        return id;
    }

    public async Task<HttpResponseMessage?> DispatchNextCallbackAsync(string operationId)
    {
        var builder = GetBuilder(operationId);

        var callback = builder.NextCallback();

        if (callback is null)
        {
            return null;
        }

        var id = callback.ProviderPaymentId ?? GetProviderPaymentId(operationId);

        _logger.LogDebug(
            "Store log. OperationId={OperationId} MethodName={MethodName} HashCode={HashCode} Callback={Callback} ProviderPaymentId={ProviderPaymentId}",
            operationId,
            nameof(DispatchNextCallbackAsync),
            GetHashCode(),
            callback,
            id);

        var response = await _callbackDispatcher.DispatchAsync(operationId, id, callback);

        return response;
    }

    public void Clear()
    {
        _builders.Clear();
        _paymentIds.Clear();
    }

    private static Guid DeterministicGuid(string operationId)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(operationId));
        return new Guid(bytes);
    }
}