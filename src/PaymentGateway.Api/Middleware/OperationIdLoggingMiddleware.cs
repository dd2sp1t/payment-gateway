using System.Diagnostics;

namespace PaymentGateway.Api.Middleware;

internal sealed class OperationIdLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<OperationIdLoggingMiddleware> _logger;

    public OperationIdLoggingMiddleware(
        RequestDelegate next,
        ILogger<OperationIdLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/metrics")
            || context.Request.Path.StartsWithSegments("/health")
            || context.Request.Path.StartsWithSegments("/swagger"))
        {
            await _next(context);
            return;
        }

        var operationId = context.Items.TryGetValue("OperationId", out var value)
            ? value?.ToString()
            : null;
        var state = new Dictionary<string, object?> { ["OperationId"] = operationId };

        using (_logger.BeginScope(state))
        {
            var start = Stopwatch.GetTimestamp();

            _logger.LogInformation(
                "HTTP request started. Method={Method} Path={Path}",
                context.Request.Method,
                context.Request.Path);

            try
            {
                await _next(context);

                var elapsed = Stopwatch.GetElapsedTime(start);

                _logger.LogInformation(
                    "HTTP request completed. Method={Method} Path={Path} StatusCode={StatusCode} DurationMs={DurationMs}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    elapsed.TotalMilliseconds);
            }
            catch (Exception exception)
            {
                var elapsed = Stopwatch.GetElapsedTime(start);

                _logger.LogError(
                    exception,
                    "HTTP request failed. Method={Method} Path={Path} DurationMs={DurationMs}",
                    context.Request.Method,
                    context.Request.Path,
                    elapsed.TotalMilliseconds);

                throw;
            }
        }
    }
}