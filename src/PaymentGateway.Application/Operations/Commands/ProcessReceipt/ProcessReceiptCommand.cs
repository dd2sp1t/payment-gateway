using MediatR;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.ProcessReceipt;

public sealed record ProcessReceiptCommand(
    Guid ProviderPaymentId,
    string OperationId,
    ReceiptResult Result,
    string Message,
    DateTimeOffset OccurredAt)
    : IRequest;