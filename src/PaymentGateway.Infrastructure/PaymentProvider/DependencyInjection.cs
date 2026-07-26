using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.PaymentProvider;

namespace PaymentGateway.Infrastructure.PaymentProvider;

public static class DependencyInjection
{
    internal static IServiceCollection AddPaymentProvider(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PaymentProviderClientOptions>(
            configuration.GetSection(nameof(PaymentProviderClientOptions)));

        services.AddHttpClient<IPaymentProviderClient, PaymentProviderClient>((sp, client) =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var options = sp.GetRequiredService<IOptions<PaymentProviderClientOptions>>().Value;

            client.BaseAddress = new Uri(
                configuration["PROVIDER_URL"]
                ?? throw new InvalidOperationException("PROVIDER_URL is not configured."));

            client.Timeout = options.Timeout;
        });

        return services;
    }
}