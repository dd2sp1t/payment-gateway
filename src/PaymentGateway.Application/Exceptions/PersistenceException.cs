namespace PaymentGateway.Application.Exceptions;

public sealed class PersistenceException(string Message, Exception? InnerException) : Exception(Message, InnerException)
{
}