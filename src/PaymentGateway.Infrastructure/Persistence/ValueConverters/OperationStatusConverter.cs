using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Infrastructure.Persistence.ValueConverters;

internal sealed class OperationStatusConverter : ValueConverter<OperationStatus, string>
{
    public OperationStatusConverter()
        : base(
            status => status.ToString().ToUpperInvariant(),
            value => Enum.Parse<OperationStatus>(value, ignoreCase: true))
    {
    }
}