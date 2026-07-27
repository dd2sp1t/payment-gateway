using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Domain.Exceptions;

public sealed class ProviderPaymentMismatchException(OperationId operationId)
    : DomainException($"Operation '{operationId}' is already linked to a different provider payment.")
{
}