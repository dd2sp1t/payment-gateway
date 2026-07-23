using PaymentGateway.Domain.Exceptions;

namespace PaymentGateway.Domain.Operations;

public readonly record struct OperationId
{
    public string Value { get; }

    public OperationId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException("Operation identifier is required.");
        }

        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(OperationId id) => id.Value;

    public static explicit operator OperationId(string value) => new(value);
}