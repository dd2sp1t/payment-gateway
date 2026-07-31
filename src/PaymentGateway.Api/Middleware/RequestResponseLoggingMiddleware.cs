namespace PaymentGateway.Api.Middleware;

internal sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        await _next(context);

        var operationId = context.Items.TryGetValue("OperationId", out var value)
            ? value?.ToString()
            : null;

        _logger.LogInformation(
            "Request completed. OperationId={OperationId} Method={Method} Path={Path} StatusCode={StatusCode}",
            operationId,
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode);
    }
}