using Microsoft.EntityFrameworkCore;
using Npgsql;
using PaymentGateway.Application.Exceptions;

namespace PaymentGateway.Infrastructure.Persistence.Mappers;

internal sealed class PersistenceExceptionMapper
{
    public Exception Map(DbUpdateException exception)
    {
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
            DatabaseConstraints.OperationsPrimaryKey =>
                new DuplicateResourceException("Operation", "OperationId"),

            DatabaseConstraints.OperationsProviderPaymentId =>
                new DuplicateResourceException("Operation", "ProviderPaymentId"),

            _ =>
                new PersistenceException("Unique constraint violation.", exception)
        };
    }
}