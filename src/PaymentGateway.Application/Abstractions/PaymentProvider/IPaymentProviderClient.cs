using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Application.Abstractions.PaymentProvider;

public interface IPaymentProviderClient
{
    Task<SubmitPaymentResponse> SubmitAsync(SubmitPaymentRequest request, CancellationToken cancellationToken);
}