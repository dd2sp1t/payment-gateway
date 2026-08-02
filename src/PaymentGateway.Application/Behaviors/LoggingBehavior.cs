using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Requests;
using PaymentGateway.Application.Helpers;

internal sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var start = Stopwatch.GetTimestamp();

        var requestName = RequestNameHelper.GetName<TRequest>();
        var operationId = (request as IOperationRequest)?.OperationId;

        _logger.LogDebug(
            "Application request started. OperationId={OperationId} ApplicationRequest={ApplicationRequest}",
            operationId,
            requestName);

        try
        {
            var response = await next();

            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogDebug(
                "Application request completed. OperationId={OperationId} ApplicationRequest={ApplicationRequest} DurationMs={DurationMs}",
                operationId,
                requestName,
                elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            var elapsed = Stopwatch.GetElapsedTime(start);
            var requestBody = JsonSerializer.Serialize(request);

            _logger.LogError(
                exception,
                "Application request failed. OperationId={OperationId} ApplicationRequest={ApplicationRequest} DurationMs={DurationMs} Body={Body}",
                operationId,
                requestName,
                elapsed.TotalMilliseconds,
                requestBody);

            throw;
        }
    }
}