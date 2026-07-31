using PaymentGateway.Domain.Operations;

namespace PaymentGateway.IntegrationTests.PaymentProvider.Steps;

public sealed record Callback(ReceiptResult Result, Guid? ProviderPaymentId = null, TimeSpan? Delay = null);