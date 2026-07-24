namespace PaymentGateway.Application.Exceptions;

public sealed class DuplicateResourceException(string resource, string field)
    : ApplicationException($"{resource} with duplicate '{field}' already exists.")
{
    public string Resource { get; } = resource;

    public string Field { get; } = field;
}