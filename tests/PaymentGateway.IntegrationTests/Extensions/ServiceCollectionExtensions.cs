using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.IntegrationTests.PaymentProvider;

namespace PaymentGateway.IntegrationTests.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection ReplaceDbContext<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : DbContext
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<TDbContext>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<TDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(TDbContext).Assembly.FullName);
            });
        });

        return services;
    }

    public static IServiceCollection ReplaceDispatchOperationsBackgroundServiceOptions(
        this IServiceCollection services)
    {
        return ReplaceOptions(services, TestOptions.Background);
    }

    public static IServiceCollection ReplaceDispatchOptions(
        this IServiceCollection services)
    {
        return ReplaceOptions(services, TestOptions.Dispatch);
    }

    public static IServiceCollection ReplacePaymentProviderClient(
        this IServiceCollection services,
        WebApplicationFactory<Program> factory)
    {
        services.AddSingleton<PaymentProviderScenarioStore>();

        services.AddSingleton(sp =>
        {
            var callbackClient = factory.CreateClient();

            return new CallbackDispatcher(
                sp.GetRequiredService<ILogger<CallbackDispatcher>>(),
                callbackClient);
        });

        services.RemoveAll<IPaymentProviderClient>();
        services.AddScoped<IPaymentProviderClient, PaymentProviderScenarioClient>();

        return services;
    }

    private static IServiceCollection ReplaceOptions<TOptions>(this IServiceCollection services, TOptions options)
        where TOptions : class, new()
    {
        services.RemoveAll<IConfigureOptions<TOptions>>();
        services.RemoveAll<IPostConfigureOptions<TOptions>>();
        services.RemoveAll<IOptions<TOptions>>();

        services.AddSingleton(Options.Create(options));

        return services;
    }
}
