namespace PaymentGateway.Application.Helpers;

internal static class RequestNameHelper
{
    public static string GetName<TRequest>()
    {
        var name = typeof(TRequest).Name;

        name = name.Replace("Command", "");
        name = name.Replace("Query", "");

        return name;
    }
}