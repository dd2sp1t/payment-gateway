using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Infrastructure.Diagnostics;
using PaymentGateway.Infrastructure.Dispatch;
using PaymentGateway.Infrastructure.PaymentProvider;
using PaymentGateway.Infrastructure.Persistence;

namespace PaymentGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddPersistence(configuration)
            .AddPaymentProvider(configuration)
            .AddDispatch(configuration)
            .AddDiagnostics();

        return services;
    }
}