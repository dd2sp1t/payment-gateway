namespace PaymentGateway.Application.Exceptions;

public sealed class ConcurrencyException : ApplicationException
{
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}