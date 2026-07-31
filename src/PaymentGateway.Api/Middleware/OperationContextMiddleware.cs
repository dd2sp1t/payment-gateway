using System.Text.Json;

namespace PaymentGateway.Api.Middleware;

internal sealed class OperationContextMiddleware
{
    private readonly RequestDelegate _next;

    public OperationContextMiddleware(RequestDelegate next)
    {
        _next = next;
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

        if (context.Request.RouteValues.TryGetValue("operationId", out var routeValue))
        {
            context.Items["OperationId"] = routeValue?.ToString();
        }
        else if (context.Request.ContentLength is > 0
            && context.Request.ContentType?.Contains("application/json") == true)
        {
            {
                context.Request.EnableBuffering();

                try
                {
                    using var document = await JsonDocument.ParseAsync(context.Request.Body);

                    if (document.RootElement.TryGetProperty("operationId", out var property))
                    {
                        context.Items["OperationId"] = property.GetString();
                    }
                }
                finally
                {
                    context.Request.Body.Position = 0;
                }
            }
        }

        await _next(context);
    }
}