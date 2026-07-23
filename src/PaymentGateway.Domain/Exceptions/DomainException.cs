namespace PaymentGateway.Domain.Exceptions;

public sealed class DomainException(string Message) : Exception(Message)
{
}