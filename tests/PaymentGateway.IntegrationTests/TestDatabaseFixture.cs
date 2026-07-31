using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.BackgroundServices.DispatchOperations;
using PaymentGateway.Infrastructure.Persistence;
using PaymentGateway.IntegrationTests.PaymentProvider;
using Testcontainers.PostgreSql;

namespace PaymentGateway.IntegrationTests;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:17")
        .WithDatabase("payment_gateway_tests")
        .WithUsername("test_user")
        .WithPassword("test_password")
        .WithCleanUp(true)
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        var connectionString = _dbContainer.GetConnectionString();

        var dispatchOptions = new DispatchOperationsBackgroundServiceOptions
        {
            Interval = TimeSpan.FromMilliseconds(500),
            BatchSize = 100,
            MaxParallelDispatches = 10
        };

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services
                        .ReplaceDbContext<PaymentGatewayDbContext>(connectionString)
                        .ReplaceOptions(dispatchOptions);

                    services.RemoveAll<PaymentProviderScenarioStore>();
                    services.AddSingleton<PaymentProviderScenarioStore>();

                    services.RemoveAll<CallbackDispatcher>();
                    services.AddSingleton(sp =>
                    {
                        var callbackClient = Factory.CreateClient();

                        return new CallbackDispatcher(
                            sp.GetRequiredService<ILogger<CallbackDispatcher>>(),
                            callbackClient);
                    });

                    services.RemoveAll<IPaymentProviderClient>();
                    services.AddScoped<IPaymentProviderClient, ScenarioPaymentProviderClient>();
                });
            });

        var optionsBuilder = new DbContextOptionsBuilder<PaymentGatewayDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        await using var db = new PaymentGatewayDbContext(optionsBuilder.Options);

        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _dbContainer.DisposeAsync();
    }
}