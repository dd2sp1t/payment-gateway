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
            name: "application_request_duration",
            unit: "ms",
            description: "Application request execution duration.");

    private readonly Counter<long> _applicationRequestConcurrencyRetry =
        Meter.CreateCounter<long>(
            name: "application_request_concurrency_retry",
            description: "Optimistic concurrency retries for application requests.");

    #endregion

    #region Operations

    private readonly Counter<long> _operationsCreated =
        Meter.CreateCounter<long>(
            name: "operations_created",
            description: "Created operations.");

    private readonly Counter<long> _operationsSubmitted =
        Meter.CreateCounter<long>(
            name: "operations_submitted",
            description: "Submitted operations.");

    private readonly Counter<long> _operationsCompleted =
        Meter.CreateCounter<long>(
            name: "operations_completed",
            description: "Completed operations.");

    private readonly Counter<long> _operationsRejected =
        Meter.CreateCounter<long>(
            name: "operations_rejected",
            description: "Rejected operations.");

    #endregion

    #region Dispatch

    private double _processingOldestAgeSeconds;

    private readonly ObservableGauge<double> _;

    private readonly Histogram<double> _dispatchBatchDuration =
        Meter.CreateHistogram<double>(
            name: "dispatch_batch_duration",
            unit: "ms",
            description: "Dispatch batch execution duration.");

    private readonly Histogram<long> _dispatchBatchSize =
        Meter.CreateHistogram<long>(
            name: "dispatch_batch_size",
            unit: "operations",
            description: "Operations processed in a dispatch batch.");

    #endregion

    #region Payment Provider

    private readonly Histogram<double> _providerRequestDuration =
        Meter.CreateHistogram<double>(
            name: "provider_request_duration",
            unit: "ms",
            description: "Payment provider request duration.");

    private readonly Counter<long> _providerDispatchRequests =
        Meter.CreateCounter<long>(
            name: "provider_dispatch_requests",
            description: "Provider dispatch requests.");

    private readonly Counter<long> _providerDispatchSucceeded =
        Meter.CreateCounter<long>(
            name: "provider_dispatch_succeeded",
            description: "Successful provider dispatches.");

    private readonly Counter<long> _providerDispatchRetryScheduled =
        Meter.CreateCounter<long>(
            name: "provider_dispatch_retry_scheduled",
            description: "Scheduled provider retries.");

    private readonly Counter<long> _providerDispatchRetryLimitReached =
        Meter.CreateCounter<long>(
            name: "provider_dispatch_retry_limit_reached",
            description: "Provider retry limit reached.");

    #endregion

    #endregion

    public Metrics()
    {
        _ = Meter.CreateObservableGauge(
            name: "processing_oldest_age",
            observeValue: () => _processingOldestAgeSeconds,
            unit: "s",
            description: "Age of the oldest processing operation.");
    }

    #region Methods

    #region Application Requests

    public IDisposable MeasureApplicationRequest(string requestName) =>
        new Timer(_applicationRequestDuration, "application_request", requestName);

    public void ApplicationRequestConcurrencyRetry(string requestName) =>
        _applicationRequestConcurrencyRetry.Add(
            1,
            new KeyValuePair<string, object?>("application_request", requestName));

    #endregion

    #region Operations

    public void OperationCreated() =>
        _operationsCreated.Add(1);

    public void OperationSubmitted() =>
        _operationsSubmitted.Add(1);

    public void OperationCompleted() =>
        _operationsCompleted.Add(1);

    public void OperationRejected() =>
        _operationsRejected.Add(1);

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

    public void DispatchRequested() =>
        _providerDispatchRequests.Add(1);

    public void DispatchSucceeded() =>
        _providerDispatchSucceeded.Add(1);

    public void DispatchRetryScheduled() =>
        _providerDispatchRetryScheduled.Add(1);

    public void DispatchRetryLimitReached() =>
        _providerDispatchRetryLimitReached.Add(1);

    #endregion

    #endregion
}