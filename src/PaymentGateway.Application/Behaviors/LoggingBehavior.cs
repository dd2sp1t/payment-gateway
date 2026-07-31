using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Requests;

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

        var requestName = typeof(TRequest).Name;
        var requestBody = JsonSerializer.Serialize(request);
        var operationId = (request as IOperationRequest)?.OperationId;

        _logger.LogDebug(
            "Handling request. OperationId={OperationId} Request={Request} Body={Body}",
            operationId,
            requestName,
            requestBody);

        try
        {
            var response = await next();

            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogDebug(
                "Request handled. OperationId={OperationId} Request={Request} DurationMs={DurationMs}",
                operationId,
                requestName,
                elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogError(
                exception,
                "Request failed. OperationId={OperationId} Request={Request} DurationMs={DurationMs} Body={Body}",
                operationId,
                requestName,
                elapsed.TotalMilliseconds,
                requestBody);

            throw;
        }
    }
}