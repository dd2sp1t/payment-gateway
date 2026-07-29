namespace PaymentGateway.IntegrationTests.PaymentProvider.Steps;

public abstract record Submit;

public sealed record SubmitAccepted(
    Guid? ProviderPaymentId = null,
    TimeSpan? Delay = null)
    : Submit;

public sealed record ServiceUnavailable() : Submit;
public sealed record GatewayTimeout() : Submit;
public sealed record TooManyRequests() : Submit;
public sealed record Timeout() : Submit;
public sealed record SocketError() : Submit;
public sealed record IoError() : Submit;
public sealed record UnexpectedError() : Submit;