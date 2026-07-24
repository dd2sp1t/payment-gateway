namespace PaymentGateway.Application.Exceptions;

public sealed class RequestValidationException(string message) : ApplicationException(message)
{
}