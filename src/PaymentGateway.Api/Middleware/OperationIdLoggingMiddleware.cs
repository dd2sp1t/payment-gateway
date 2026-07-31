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
        var operationId = context.Items.TryGetValue("OperationId", out var value)
            ? value?.ToString()
            : null;

        var state = new Dictionary<string, object?> { ["OperationId"] = operationId };
        using (_logger.BeginScope(state))
        {
            await _next(context);

            if (context.Request.Path.StartsWithSegments("/metrics")
                || context.Request.Path.StartsWithSegments("/health")
                || context.Request.Path.StartsWithSegments("/swagger"))
            {
                return;
            }

            _logger.LogInformation(
                "Request completed. Method={Method} Path={Path} StatusCode={StatusCode}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode);
        }
    }
}