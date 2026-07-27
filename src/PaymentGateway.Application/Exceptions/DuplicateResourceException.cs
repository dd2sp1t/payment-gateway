public sealed class DuplicateResourceException : PaymentGateway.Application.Exceptions.ApplicationException
{
    public DuplicateResourceException(string resource, params string[] properties)
        : base($"Duplicate '{resource}' for unique field(s): {string.Join(", ", properties)}.")
    {
        Resource = resource;
        Properties = properties;
    }

    public string Resource { get; }

    public IReadOnlyList<string> Properties { get; }
}