using System.Collections.Concurrent;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal sealed class PaymentProviderScenarioBuilder
{
    private readonly ConcurrentQueue<PaymentProviderScenarioStep> _steps = new();

    public PaymentProviderScenarioBuilder Accepted()
    {
        _steps.Enqueue(new(PaymentProviderScenario.Accepted));
        return this;
    }

    public PaymentProviderScenarioBuilder AcceptedNewPaymentId()
    {
        _steps.Enqueue(new(PaymentProviderScenario.AcceptedNewPaymentId));
        return this;
    }

    public PaymentProviderScenarioBuilder ServiceUnavailable(int count = 1)
    {
        Enqueue(PaymentProviderScenario.ServiceUnavailable, count);
        return this;
    }

    public PaymentProviderScenarioBuilder GatewayTimeout(int count = 1)
    {
        Enqueue(PaymentProviderScenario.GatewayTimeout, count);
        return this;
    }

    public PaymentProviderScenarioBuilder TooManyRequests(int count = 1)
    {
        Enqueue(PaymentProviderScenario.TooManyRequests, count);
        return this;
    }

    public PaymentProviderScenarioBuilder Timeout(int count = 1)
    {
        Enqueue(PaymentProviderScenario.Timeout, count);
        return this;
    }

    public PaymentProviderScenarioBuilder SocketError(int count = 1)
    {
        Enqueue(PaymentProviderScenario.SocketError, count);
        return this;
    }

    public PaymentProviderScenarioBuilder IoError(int count = 1)
    {
        Enqueue(PaymentProviderScenario.IoError, count);
        return this;
    }

    public PaymentProviderScenarioBuilder UnexpectedError(int count = 1)
    {
        Enqueue(PaymentProviderScenario.UnexpectedError, count);
        return this;
    }

    private void Enqueue(PaymentProviderScenario scenario, int count)
    {
        for (var i = 0; i < count; i++)
        {
            _steps.Enqueue(new(scenario));
        }
    }

    internal PaymentProviderScenarioStep Next()
    {
        if (_steps.TryDequeue(out var step))
        {
            return step;
        }

        return new(PaymentProviderScenario.Accepted);
    }
}