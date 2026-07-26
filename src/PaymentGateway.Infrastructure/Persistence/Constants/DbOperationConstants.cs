namespace PaymentGateway.Infrastructure.Persistence.Constants;

internal static class DbOperationConstants
{
    public const int OperationIdLength = 128;
    public const int AmountPrecision = 18;
    public const int AmountScale = 2;
    public const int CurrencyLength = 3;
    public const int DescriptionLength = 512;
    public const int StatusLength = 16;
}