namespace PaymentGateway.Application.Abstractions.PaymentProvider.Models;

public sealed record SubmitPaymentResponse(
    Guid ProviderPaymentId,
    ProviderPaymentStatus Status);