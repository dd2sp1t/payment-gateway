using MediatR;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Helpers;

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
        var requestName = RequestNameHelper.GetName<TRequest>();

        using var _ = _metrics.MeasureApplicationRequest(requestName);

        _metrics.ApplicationRequestStarted(requestName);

        try
        {
            var task = await next();

            _metrics.ApplicationRequestSucceeded(requestName);

            return task;
        }
        catch (Exception)
        {
            _metrics.ApplicationRequestFailed(requestName);

            throw;
        }
    }
}