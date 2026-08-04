using System.Diagnostics.Metrics;
using PaymentGateway.Application.Abstractions.Diagnostics;

namespace PaymentGateway.Infrastructure.Diagnostics;

internal sealed class Metrics : IMetrics
{
    #region Fields

    private static readonly Meter Meter = new(Telemetry.MeterName);

    #region Application Requests

    private readonly Histogram<double> _applicationRequestDuration =
        Meter.CreateHistogram<double>(
            name: "paymentgateway.application.request.duration",
            unit: "ms",
            description: "Application request execution duration.");

    private readonly Counter<long> _applicationRequestAttempts =
        Meter.CreateCounter<long>(
            name: "paymentgateway.application.request.attempt",
            description: "Application request attempts.");

    private readonly Counter<long> _applicationRequestRetries =
        Meter.CreateCounter<long>(
            name: "paymentgateway.application.request.retry",
            description: "Application request retries.");

    #endregion

    #region Operations

    private readonly Counter<long> _operationEvents =
        Meter.CreateCounter<long>(
            name: "paymentgateway.operation.event",
            description: "Operation lifecycle events.");

    #endregion

    #region Dispatch

    private double _processingOldestAgeSeconds;

    private readonly ObservableGauge<double> _;

    private readonly Histogram<double> _dispatchBatchDuration =
        Meter.CreateHistogram<double>(
            name: "paymentgateway.dispatch.batch.duration",
            unit: "ms",
            description: "Dispatch batch execution duration.");

    private readonly Histogram<long> _dispatchBatchSize =
        Meter.CreateHistogram<long>(
            name: "paymentgateway.dispatch.batch.size",
            unit: "operations",
            description: "Operations processed in a dispatch batch.");

    #endregion

    #region Payment Provider

    private readonly Histogram<double> _providerRequestDuration =
        Meter.CreateHistogram<double>(
            "paymentgateway.provider.request.duration",
            unit: "ms",
            description: "Payment provider request duration.");

    private readonly Counter<long> _providerDispatchAttempts =
        Meter.CreateCounter<long>(
            "paymentgateway.provider.dispatch.attempt",
            description: "Provider dispatch attempts.");

    private readonly Counter<long> _providerDispatchRetries =
        Meter.CreateCounter<long>(
            "paymentgateway.provider.dispatch.retry",
            description: "Provider dispatch retries.");

    #endregion

    #endregion

    public Metrics()
    {
        _ = Meter.CreateObservableGauge(
            name: "paymentgateway.dispatch.processing.oldest.age",
            observeValue: () => _processingOldestAgeSeconds,
            unit: "s",
            description: "Age of the oldest processing operation.");
    }

    #region Methods

    #region Application Requests

    public IDisposable MeasureApplicationRequest(string requestName) =>
        new Timer(
            histogram: _applicationRequestDuration,
            tagName: "request.name",
            requestName);

    public void ApplicationRequestStarted(string requestName) =>
        _applicationRequestAttempts.Add(
            delta: 1,
            tag1: new("request.name", requestName),
            tag2: new("event", "started"));

    public void ApplicationRequestSucceeded(string requestName) =>
        _applicationRequestAttempts.Add(
            delta: 1,
            tag1: new("request.name", requestName),
            tag2: new("event", "succeeded"));

    public void ApplicationRequestFailed(string requestName) =>
        _applicationRequestAttempts.Add(
            delta: 1,
            tag1: new("request.name", requestName),
            tag2: new("event", "failed"));

    public void ApplicationRequestConcurrencyRetry(string requestName) =>
        _applicationRequestRetries.Add(
            delta: 1,
            tag1: new("request.name", requestName),
            tag2: new("reason", "optimistic_concurrency"));

    #endregion

    #region Operations

    public void OperationCreated() =>
        _operationEvents.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "created"));

    public void OperationSubmitted() =>
        _operationEvents.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "submitted"));

    public void OperationCompleted() =>
        _operationEvents.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "completed"));

    public void OperationRejected() =>
        _operationEvents.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "rejected"));

    #endregion

    #region Dispatch

    public IDisposable MeasureDispatchBatch() =>
        new Timer(_dispatchBatchDuration);

    public void DispatchBatch(int batchSize) =>
        _dispatchBatchSize.Record(batchSize);

    public void ProcessingOldestAge(TimeSpan? age) =>
        _processingOldestAgeSeconds = age?.TotalSeconds ?? 0;

    #endregion

    #region Payment Provider

    public IDisposable MeasureProviderRequest() =>
        new Timer(_providerRequestDuration);

    public void ProviderDispatchStarted() =>
        _providerDispatchAttempts.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "started"));

    public void ProviderDispatchSucceeded() =>
        _providerDispatchAttempts.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "succeeded"));

    public void ProviderDispatchFailed() =>
        _providerDispatchAttempts.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "failed"));

    public void ProviderDispatchRetryScheduled() =>
        _providerDispatchRetries.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "scheduled"));

    public void ProviderDispatchRetryLimitReached() =>
        _providerDispatchRetries.Add(
            delta: 1,
            tag: new KeyValuePair<string, object?>("event", "limit_reached"));

    #endregion

    #endregion
}