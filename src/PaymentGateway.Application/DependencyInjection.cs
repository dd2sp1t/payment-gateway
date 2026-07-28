using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Abstractions.BackgroundServices.DispatchOperations;
using PaymentGateway.Application.Behaviors;

namespace PaymentGateway.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        services.AddValidatorsFromAssembly(assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(MetricsBehavior<,>));

        services.Configure<DispatchOperationsBackgroundServiceOptions>(
            configuration.GetSection(nameof(DispatchOperationsBackgroundServiceOptions)));

        services.AddHostedService<DispatchOperationsBackgroundService>();

        return services;
    }
}