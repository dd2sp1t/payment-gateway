using System.Net.Sockets;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Tests;

internal sealed class AlwaysFailPaymentProviderClient : IPaymentProviderClient
{
    public AlwaysFailPaymentProviderClient(HttpClient _)
    {
    }

    public Task<SubmitPaymentResponse> SubmitAsync(
        SubmitPaymentRequest request,
        int retryAttempt,
        CancellationToken cancellationToken)
    {
        throw new SocketException((int)SocketError.HostUnreachable);
    }
}