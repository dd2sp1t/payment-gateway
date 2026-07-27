using System.Net.Sockets;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Tests;

internal sealed class FailNTimesPaymentProviderClient : IPaymentProviderClient
{
    private static int _remainingFailures = 2;

    public FailNTimesPaymentProviderClient(HttpClient _)
    {
    }

    public Task<SubmitPaymentResponse> SubmitAsync(
        SubmitPaymentRequest request,
        int retryAttempt,
        CancellationToken cancellationToken)
    {
        if (_remainingFailures-- > 0)
        {
            throw new SocketException((int)SocketError.HostUnreachable);
        }

        return Task.FromResult(new SubmitPaymentResponse(Guid.NewGuid(), ProviderPaymentStatus.Accepted));
    }
}