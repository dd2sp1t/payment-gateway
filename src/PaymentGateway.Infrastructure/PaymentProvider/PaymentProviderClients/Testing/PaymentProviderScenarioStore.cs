using System.Collections.Concurrent;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal sealed class PaymentProviderScenarioStore
{
    private readonly ConcurrentDictionary<string, PaymentProviderScenarioBuilder> _builders = new();
    private readonly ConcurrentDictionary<string, Guid> _paymentIds = new();

    public PaymentProviderScenarioBuilder For(string operationId)
    {
        return _builders.GetOrAdd(
            operationId,
            _ => new PaymentProviderScenarioBuilder());
    }

    internal PaymentProviderScenarioBuilder Get(string operationId)
    {
        return _builders.GetOrAdd(
            operationId,
            _ => new PaymentProviderScenarioBuilder());
    }

    internal Guid GetPaymentId(string operationId)
    {
        return _paymentIds.GetOrAdd(
            operationId,
            _ => Guid.NewGuid());
    }

    internal Guid GetNewPaymentId()
    {
        return Guid.NewGuid();
    }

    public void Clear()
    {
        _builders.Clear();
        _paymentIds.Clear();
    }
}