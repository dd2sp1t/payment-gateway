using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Mappers;

internal sealed class ReceiptMapper
{
    public DbReceipt ToEntity(Receipt receipt)
    {
        return new DbReceipt
        {
            ReceiptId = receipt.ReceiptId,
            ProviderPaymentId = receipt.ProviderPaymentId,
            OperationId = receipt.OperationId,
            Result = receipt.Result,
            Message = receipt.Message,
            OccurredAt = receipt.OccurredAt
        };
    }
}