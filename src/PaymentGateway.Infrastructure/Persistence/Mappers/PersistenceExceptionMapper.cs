using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Conventions;

namespace PaymentGateway.Infrastructure.Persistence.Mappers;

internal sealed class PersistenceExceptionMapper
{
    public Exception Map(DbUpdateException exception)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            return new ConcurrencyException("Database concurrency conflict.", exception);
        }

        if (exception.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return MapUniqueViolation(postgresException);
        }

        return new PersistenceException("Database operation failed.", exception);
    }

    private Exception MapUniqueViolation(PostgresException exception)
    {
        return exception.ConstraintName switch
        {
            var name when name == ConstraintNames.OperationsPrimaryKey =>
                new DuplicateResourceException(nameof(Operation), nameof(Operation.OperationId)),

            var name when name == ConstraintNames.OperationsProviderPaymentIdUnique =>
                new DuplicateResourceException(nameof(Operation), nameof(Operation.ProviderPaymentId)),

            var name when name == ConstraintNames.OperationEventsPrimaryKey =>
                new DuplicateResourceException(
                    nameof(OperationEvent),
                    nameof(OperationEvent.OperationId),
                    nameof(OperationEvent.EventId)),

            var name when name == ConstraintNames.ReceiptsPrimaryKey =>
                new DuplicateResourceException(
                    nameof(Receipt),
                    nameof(Receipt.ReceiptId)),

            var name when name == ConstraintNames.ReceiptsOperationProviderPaymentResultUnique =>
                new DuplicateResourceException(
                    nameof(Receipt),
                    nameof(Receipt.OperationId),
                    nameof(Receipt.ProviderPaymentId),
                    nameof(Receipt.Result)),

            _ => new PersistenceException("Unique constraint violation.", exception)
        };
    }
}