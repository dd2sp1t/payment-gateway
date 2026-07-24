using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Mappers;

internal sealed class OperationMapper
{
    public Operation ToDomain(DbOperation dbOperation)
    {
        return Operation.Restore(
            operationId: (OperationId)dbOperation.OperationId,
            providerPaymentId: dbOperation.ProviderPaymentId,
            amount: dbOperation.Amount,
            currency: dbOperation.Currency,
            description: dbOperation.Description,
            status: dbOperation.Status);
    }

    public DbOperation ToEntity(Operation operation)
    {
        return new DbOperation
        {
            OperationId = operation.OperationId,
            ProviderPaymentId = operation.ProviderPaymentId,
            Amount = operation.Amount,
            Currency = operation.Currency,
            Description = operation.Description,
            Status = operation.Status
        };
    }

    public void Apply(Operation operation, DbOperation dbOperation)
    {
        dbOperation.ProviderPaymentId = operation.ProviderPaymentId;
        dbOperation.Amount = operation.Amount;
        dbOperation.Currency = operation.Currency;
        dbOperation.Description = operation.Description;
        dbOperation.Status = operation.Status;
    }
}