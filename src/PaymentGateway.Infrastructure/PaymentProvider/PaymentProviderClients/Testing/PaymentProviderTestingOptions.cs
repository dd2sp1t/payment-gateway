namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

public sealed class PaymentProviderTestingOptions
{
    public int OperationCount { get; init; }
    public int MaxRetryCount { get; init; }
    public int LastOperationId { get; init; }
}