namespace PaymentGateway.Infrastructure.PaymentProvider;

public sealed class PaymentProviderClientOptions
{
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(30);
}