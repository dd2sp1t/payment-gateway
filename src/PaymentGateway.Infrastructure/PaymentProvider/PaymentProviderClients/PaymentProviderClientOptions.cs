namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients;

public sealed class PaymentProviderClientOptions
{
    public TimeSpan Timeout { get; init; }
}