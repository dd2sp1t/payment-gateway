using System.Net;
using System.Net.Sockets;
using PaymentGateway.Application.Abstractions.Dispatch;

namespace PaymentGateway.Infrastructure.Dispatch;

internal sealed class DispatchFailureClassifier : IDispatchFailureClassifier
{
    public bool IsTransient(Exception exception)
    {
        return exception switch
        {
            TimeoutException => true,

            HttpRequestException http
                when http.StatusCode == HttpStatusCode.ServiceUnavailable =>
                true,

            HttpRequestException http
                when http.StatusCode == HttpStatusCode.GatewayTimeout =>
                true,

            HttpRequestException http
                when http.StatusCode == HttpStatusCode.TooManyRequests =>
                true,

            SocketException => true,

            IOException => true,

            _ => false
        };
    }
}