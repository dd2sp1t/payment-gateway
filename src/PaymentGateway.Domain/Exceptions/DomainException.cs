namespace PaymentGateway.Domain.Exceptions;

public class DomainException(string Message) : Exception(Message)
{
}