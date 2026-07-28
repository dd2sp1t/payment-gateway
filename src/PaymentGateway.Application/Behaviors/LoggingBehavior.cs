using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

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

        _logger.LogDebug(
            "Handling request. Request={Request} Body={Body}",
            requestName,
            requestBody);

        try
        {
            var response = await next();

            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogDebug(
                "Request handled. Request={Request} DurationMs={DurationMs}",
                requestName,
                elapsed.TotalMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            var elapsed = Stopwatch.GetElapsedTime(start);

            _logger.LogError(
                exception,
                "Request failed. Request={Request} DurationMs={DurationMs} Body={Body}",
                requestName,
                elapsed.TotalMilliseconds,
                requestBody);

            throw;
        }
    }
}