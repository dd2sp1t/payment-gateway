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
        _logger.LogInformation("{ServiceName} started.", GetType().Name);

        while (stoppingToken.IsCancellationRequested == false)
        {
            try
            {
                await ExecuteIterationAsync(stoppingToken);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "{ServiceName} iteration failed.", GetType().Name);
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("{ServiceName} stopped.", GetType().Name);
    }

    protected abstract Task ExecuteIterationAsync(CancellationToken cancellationToken);
}