namespace PaymentGateway.Application.Abstractions.PaymentProvider;

public sealed class PaymentProviderException : ApplicationException
{
    public PaymentProviderException(string message)
        : base(message)
    {
    }

    public PaymentProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}