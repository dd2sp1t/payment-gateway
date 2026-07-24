namespace PaymentGateway.Application.Exceptions;

public sealed class ConflictException(string message) : ApplicationException(message)
{
}