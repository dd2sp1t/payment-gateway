using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Application.Abstractions.BackgroundServices;

internal abstract class PeriodicBackgroundService : BackgroundService
{
    private readonly ILogger _logger;
    private readonly TimeSpan _interval;

    protected PeriodicBackgroundService(ILogger logger, TimeSpan interval)
    {
        _logger = logger;
        _interval = interval;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var serviceName = GetType().Name;

        _logger.LogInformation(
            "Service started. ServiceName={ServiceName}",
            serviceName);

        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                await ExecuteIterationAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Service iteration failed. ServiceName={ServiceName}",
                    serviceName);
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation(
            "Service stopped. ServiceName={ServiceName}",
            serviceName);
    }

    protected abstract Task ExecuteIterationAsync(CancellationToken cancellationToken);
}