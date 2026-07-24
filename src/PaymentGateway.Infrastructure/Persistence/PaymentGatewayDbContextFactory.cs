using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PaymentGateway.Infrastructure.Persistence;

internal sealed class PaymentGatewayDbContextFactory : IDesignTimeDbContextFactory<PaymentGatewayDbContext>
{
    public PaymentGatewayDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../PaymentGateway.Api"))
            .AddJsonFile("appsettings.Development.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<PaymentGatewayDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            options =>
            {
                options.MigrationsAssembly(typeof(PaymentGatewayDbContext).Assembly.FullName);
            });


        return new PaymentGatewayDbContext(optionsBuilder.Options);
    }
}