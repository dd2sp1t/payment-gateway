using MediatR;
using PaymentGateway.Application.Abstractions.Diagnostics;

namespace PaymentGateway.Application.Behaviors;

internal sealed class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IMetrics _metrics;

    public MetricsBehavior(IMetrics metrics)
    {
        _metrics = metrics;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        using var _ = _metrics.MeasureApplicationRequest(GetName());

        return await next();
    }

    private static string GetName()
    {
        var name = typeof(TRequest).Name;

        name = name.Replace("Command", "");
        name = name.Replace("Query", "");

        return name;
    }
}