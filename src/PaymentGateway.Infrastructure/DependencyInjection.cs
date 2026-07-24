using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Infrastructure.Persistence;
using PaymentGateway.Infrastructure.Persistence.Mappers;
using PaymentGateway.Infrastructure.Persistence.Repositories;

namespace PaymentGateway.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' was not found.");

        services.AddDbContext<PaymentGatewayDbContext>(options =>
        {
            options.UseNpgsql(
                connectionString,
                npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(PaymentGatewayDbContext).Assembly.FullName);
                });
        });

        services.AddSingleton<PersistenceExceptionMapper>();
        services.AddSingleton<OperationMapper>();

        services.AddScoped<IOperationRepository, OperationRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}