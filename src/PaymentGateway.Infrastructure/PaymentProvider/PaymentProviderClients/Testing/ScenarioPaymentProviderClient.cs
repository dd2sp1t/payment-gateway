using System.Net;
using System.Net.Sockets;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal sealed class ScenarioPaymentProviderClient : IPaymentProviderClient
{
    private readonly PaymentProviderScenarioStore _store;

    public ScenarioPaymentProviderClient(HttpClient _, PaymentProviderScenarioStore store)
    {
        _store = store;
    }

    public Task<SubmitPaymentResponse> SubmitAsync(
        SubmitPaymentRequest request,
        int retryAttempt,
        CancellationToken cancellationToken)
    {
        var scenario = _store
            .Get(request.OperationId)
            .Next()
            .Scenario;

        return scenario switch
        {
            PaymentProviderScenario.Accepted =>
                Task.FromResult(
                    new SubmitPaymentResponse(
                        ProviderPaymentId: _store.GetPaymentId(request.OperationId),
                        Status: ProviderPaymentStatus.Accepted)),

            PaymentProviderScenario.AcceptedNewPaymentId =>
                Task.FromResult(
                    new SubmitPaymentResponse(
                        ProviderPaymentId: _store.GetNewPaymentId(),
                        Status: ProviderPaymentStatus.Accepted)),

            PaymentProviderScenario.ServiceUnavailable =>
                throw new HttpRequestException(
                    message: "503",
                    inner: null,
                    statusCode: HttpStatusCode.ServiceUnavailable),

            PaymentProviderScenario.GatewayTimeout =>
                throw new HttpRequestException(
                    message: "504",
                    inner: null,
                    statusCode: HttpStatusCode.GatewayTimeout),

            PaymentProviderScenario.TooManyRequests =>
                throw new HttpRequestException(
                    message: "429",
                    inner: null,
                    statusCode: HttpStatusCode.TooManyRequests),

            PaymentProviderScenario.Timeout => throw new TimeoutException(),

            PaymentProviderScenario.SocketError => throw new SocketException(),

            PaymentProviderScenario.IoError => throw new IOException(),

            PaymentProviderScenario.UnexpectedError => throw new Exception(),

            _ =>
                throw new ArgumentOutOfRangeException()
        };
    }
}