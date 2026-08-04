using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using PaymentGateway.Application.Abstractions.Diagnostics;

namespace PaymentGateway.Infrastructure.Diagnostics;

internal static class DependencyInjection
{
    public static IServiceCollection AddDiagnostics(this IServiceCollection services)
    {
        services
            .AddOpenTelemetry()
            .WithMetrics(builder =>
            {
                builder
                    .AddMeter(Telemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddPrometheusExporter();
            });

        services.AddSingleton<IMetrics, Metrics>();

        return services;
    }
}