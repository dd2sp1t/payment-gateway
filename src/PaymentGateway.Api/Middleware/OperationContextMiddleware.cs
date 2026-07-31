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
        if (context.Request.RouteValues.TryGetValue("operationId", out var routeValue))
        {
            context.Items["OperationId"] = routeValue?.ToString();
        }
        else
        {
            context.Request.EnableBuffering();

            using var document = await JsonDocument.ParseAsync(context.Request.Body);

            if (document.RootElement.TryGetProperty("operationId", out var property))
            {
                context.Items["OperationId"] = property.GetString();
            }

            context.Request.Body.Position = 0;
        }

        await _next(context);
    }
}