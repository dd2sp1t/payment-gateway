namespace PaymentGateway.Application.Exceptions;

public sealed class PersistenceException(string message, Exception innerException)
    : ApplicationException(message, innerException)
{
}