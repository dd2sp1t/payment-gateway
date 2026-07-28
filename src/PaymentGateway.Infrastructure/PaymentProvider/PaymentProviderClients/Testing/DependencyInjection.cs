using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.PaymentProvider;

namespace PaymentGateway.Infrastructure.PaymentProvider.PaymentProviderClients.Testing;

internal static class DependencyInjection
{
    public static IServiceCollection AddScenarioPaymentProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PaymentProviderTestingOptions>(
            configuration.GetSection(nameof(PaymentProviderTestingOptions)));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PaymentProviderTestingOptions>>().Value;

            var store = new PaymentProviderScenarioStore();

            store.SeedRandom(options.OperationCount, options.MaxRetryCount, options.LastOperationId);

            return store;
        });

        services.AddHttpClient<IPaymentProviderClient, ScenarioPaymentProviderClient>((sp, client) => { });

        return services;
    }
}