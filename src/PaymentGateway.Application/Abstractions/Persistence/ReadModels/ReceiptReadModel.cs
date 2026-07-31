using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public sealed record ReceiptReadModel(
    Guid ProviderPaymentId,
    ReceiptResult Result,
    string Message,
    DateTimeOffset OccurredAt);