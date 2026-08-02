using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Persistence;
using PaymentGateway.IntegrationTests.Extensions;
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

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureSerilogForTests();

                    builder.ConfigureServices(services =>
                    {
                        services
                            .ReplaceDbContext<PaymentGatewayDbContext>(connectionString)
                            .ReplaceDispatchOperationsBackgroundServiceOptions()
                            .ReplaceDispatchOptions()
                            .ReplacePaymentProviderClient(Factory);
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

        await Serilog.Log.CloseAndFlushAsync();

        await _dbContainer.DisposeAsync();
    }
}