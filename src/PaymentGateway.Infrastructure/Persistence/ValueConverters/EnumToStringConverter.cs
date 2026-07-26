using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace PaymentGateway.Infrastructure.Persistence.ValueConverters;

internal sealed class EnumToStringConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumToStringConverter()
        : base(
            status => status.ToString().ToUpperInvariant(),
            value => Enum.Parse<TEnum>(value, ignoreCase: true))
    {
    }
}