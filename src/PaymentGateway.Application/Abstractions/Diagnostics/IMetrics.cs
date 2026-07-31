namespace PaymentGateway.Application.Abstractions.Diagnostics;

public interface IMetrics
{
    #region Application Requests

    IDisposable MeasureApplicationRequest(string requestName);
    void ApplicationRequestConcurrencyRetry(string requestName);

    #endregion

    #region Operations

    void OperationCreated();
    void OperationSubmitted();
    void OperationCompleted();
    void OperationRejected();

    #endregion

    #region Dispatch

    IDisposable MeasureDispatchBatch();

    void DispatchBatch(int batchSize);

    void ProcessingOldestAge(TimeSpan? age);

    #endregion

    #region Payment Provider

    IDisposable MeasureProviderRequest();

    void DispatchRequested();
    void DispatchSucceeded();

    void DispatchRetryScheduled();
    void DispatchRetryLimitReached();

    #endregion
}