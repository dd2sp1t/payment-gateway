using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Infrastructure.Persistence.Mappers;
using PaymentGateway.Infrastructure.Persistence.ReadRepositories;
using PaymentGateway.Infrastructure.Persistence.Repositories;

namespace PaymentGateway.Infrastructure.Persistence;

public static class DependencyInjection
{
    internal static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
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
        services.AddSingleton<OperationEventMapper>();
        services.AddSingleton<ReceiptMapper>();

        services.AddScoped<IOperationRepository, OperationRepository>();
        services.AddScoped<IOperationReadRepository, OperationReadRepository>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}