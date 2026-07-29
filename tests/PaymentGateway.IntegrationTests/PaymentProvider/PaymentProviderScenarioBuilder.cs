using System.Collections.Concurrent;
using PaymentGateway.Domain.Operations;
using PaymentGateway.IntegrationTests.PaymentProvider.Steps;

namespace PaymentGateway.IntegrationTests.PaymentProvider;

public sealed class PaymentProviderScenarioBuilder
{
    private readonly ConcurrentQueue<Submit> _submits = new();
    private readonly ConcurrentQueue<Callback> _callbacks = new();

    public PaymentProviderScenarioBuilder SubmitAccepted(Guid? providerPaymentId = null, TimeSpan? delay = null)
    {
        _submits.Enqueue(new SubmitAccepted(providerPaymentId, delay));
        return this;
    }

    public PaymentProviderScenarioBuilder Callback(
        ReceiptResult result,
        Guid? providerPaymentId = null,
        TimeSpan? delay = null)
    {
        _callbacks.Enqueue(new Callback(result, providerPaymentId, delay));
        return this;
    }

    public PaymentProviderScenarioBuilder ServiceUnavailable()
    {
        _submits.Enqueue(new ServiceUnavailable());
        return this;
    }

    public PaymentProviderScenarioBuilder GatewayTimeout()
    {
        _submits.Enqueue(new GatewayTimeout());
        return this;
    }

    public PaymentProviderScenarioBuilder TooManyRequests()
    {
        _submits.Enqueue(new TooManyRequests());
        return this;
    }

    public PaymentProviderScenarioBuilder Timeout()
    {
        _submits.Enqueue(new Steps.Timeout());
        return this;
    }

    public PaymentProviderScenarioBuilder SocketError()
    {
        _submits.Enqueue(new SocketError());
        return this;
    }

    public PaymentProviderScenarioBuilder IoError()
    {
        _submits.Enqueue(new IoError());
        return this;
    }

    public PaymentProviderScenarioBuilder UnexpectedError()
    {
        _submits.Enqueue(new UnexpectedError());
        return this;
    }

    internal Submit? NextSubmit()
    {
        _submits.TryDequeue(out var submit);
        return submit;
    }

    internal Callback? NextCallback()
    {
        _callbacks.TryDequeue(out var callback);
        return callback;
    }

    internal Callback? PeekCallback()
    {
        _callbacks.TryPeek(out var callback);
        return callback;
    }
}