using System.Globalization;

namespace PaymentGateway.Application.Extensions;

internal static class DecimalExtensions
{
    public static string ToInvariantString(this decimal value)
    {
        return value.ToString("0.00", CultureInfo.InvariantCulture);
    }
}