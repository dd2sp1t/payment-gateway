namespace PaymentGateway.Application.Abstractions.Diagnostics;

public interface IMetrics
{
    #region Application Requests

    IDisposable MeasureApplicationRequest(string requestName);

    void ApplicationRequestStarted(string requestName);
    void ApplicationRequestSucceeded(string requestName);
    void ApplicationRequestFailed(string requestName);
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

    void ProviderDispatchStarted();
    void ProviderDispatchSucceeded();
    void ProviderDispatchFailed();
    void ProviderDispatchRetryScheduled();
    void ProviderDispatchRetryLimitReached();

    #endregion
}