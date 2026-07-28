namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal enum PaymentProviderScenario
{
    Accepted,
    AcceptedNewPaymentId,

    // HTTP transient
    ServiceUnavailable,
    GatewayTimeout,
    TooManyRequests,

    // Network transient
    Timeout,
    SocketError,
    IoError,

    UnexpectedError
}