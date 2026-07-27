using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Abstractions.Dispatch;

namespace PaymentGateway.Infrastructure.Dispatch;

public static class DependencyInjection
{
    public static IServiceCollection AddDispatch(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DispatchOptions>(configuration.GetSection(nameof(DispatchOptions)));

        services.AddSingleton<IDispatchRetryPolicy, ExponentialBackoffDispatchRetryPolicy>();

        services.AddSingleton<IDispatchFailureClassifier, DispatchFailureClassifier>();

        return services;
    }
}