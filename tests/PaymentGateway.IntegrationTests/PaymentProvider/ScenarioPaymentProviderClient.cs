using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;
using PaymentGateway.IntegrationTests.PaymentProvider.Steps;

namespace PaymentGateway.IntegrationTests.PaymentProvider;

public sealed class ScenarioPaymentProviderClient : IPaymentProviderClient
{
    private readonly ILogger<ScenarioPaymentProviderClient> _logger;
    private readonly PaymentProviderScenarioStore _store;

    public ScenarioPaymentProviderClient(
        ILogger<ScenarioPaymentProviderClient> logger,
        PaymentProviderScenarioStore store)
    {
        _logger = logger;
        _store = store;
    }

    public async Task<SubmitPaymentResponse> SubmitAsync(
        SubmitPaymentRequest request,
        int retryAttempt,
        CancellationToken cancellationToken)
    {
        var step = _store.NextSubmit(request.OperationId);

        switch (step)
        {
            case SubmitAccepted submit:
                {
                    var providerPaymentId = submit.ProviderPaymentId ?? _store.GetProviderPaymentId(request.OperationId);

                    if (submit.Delay.HasValue)
                    {
                        _logger.LogInformation(
                            "Submit delayed. OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} DelayMs={DelayMs}",
                            request.OperationId,
                            providerPaymentId,
                            submit.Delay.Value.TotalMilliseconds);

                        await Task.Delay(submit.Delay.Value, CancellationToken.None);
                    }

                    _logger.LogInformation(
                        "Submit accepted . OperationId={OperationId} ProviderPaymentId={ProviderPaymentId} RetryCount={RetryCount}",
                        request.OperationId,
                        providerPaymentId,
                        retryAttempt);

                    return new SubmitPaymentResponse(providerPaymentId, ProviderPaymentStatus.Accepted);
                }

            case ServiceUnavailable:
                LogProviderError(request.OperationId, retryAttempt, "503");
                throw new HttpRequestException("503", null, HttpStatusCode.ServiceUnavailable);

            case GatewayTimeout:
                LogProviderError(request.OperationId, retryAttempt, "504");
                throw new HttpRequestException("504", null, HttpStatusCode.GatewayTimeout);

            case TooManyRequests:
                LogProviderError(request.OperationId, retryAttempt, "429");
                throw new HttpRequestException("429", null, HttpStatusCode.TooManyRequests);

            case Steps.Timeout:
                LogProviderTimeout(request.OperationId, retryAttempt);
                throw new TimeoutException();

            case Steps.SocketError:
                LogProviderSocketError(request.OperationId, retryAttempt);
                throw new SocketException();

            case IoError:
                LogProviderIoError(request.OperationId, retryAttempt);
                throw new IOException();

            case UnexpectedError:
                LogProviderUnexpectedError(request.OperationId, retryAttempt);
                throw new Exception();

            case null:
                LogScenarioConfigWarning(request.OperationId);
                throw new InvalidOperationException();

            default:
                LogUnsupportedSubmit(request.OperationId, step.GetType().Name);
                throw new InvalidOperationException();
        }
    }

    private void LogProviderError(string operationId, int retryCount, string statusCode)
    {
        _logger.LogWarning(
            "Scenario error. OperationId={OperationId} RetryCount={RetryCount} StatusCode={StatusCode}",
            operationId,
            retryCount,
            statusCode);
    }

    private void LogProviderTimeout(string operationId, int retryCount)
    {
        _logger.LogWarning(
            "Scenario timeout. OperationId={OperationId} RetryCount={RetryCount}",
            operationId,
            retryCount);
    }

    private void LogProviderSocketError(string operationId, int retryCount)
    {
        _logger.LogWarning(
            "Scenario socket error. OperationId={OperationId} RetryCount={RetryCount}",
            operationId,
            retryCount);
    }

    private void LogProviderIoError(string operationId, int retryCount)
    {
        _logger.LogWarning(
            "Scenario IO error. OperationId={OperationId} RetryCount={RetryCount}",
            operationId,
            retryCount);
    }

    private void LogProviderUnexpectedError(string operationId, int retryCount)
    {
        _logger.LogError(
            "Scenario unexpected error. OperationId={OperationId} RetryCount={RetryCount}",
            operationId,
            retryCount);
    }

    private void LogScenarioConfigWarning(string operationId)
    {
        _logger.LogWarning(
            "No submit configured. OperationId={OperationId}",
            operationId);
    }

    private void LogUnsupportedSubmit(string operationId, string submit)
    {
        _logger.LogError(
            "Submit unsupported. OperationId={OperationId} Submit={Submit}",
            operationId,
            submit);
    }
}