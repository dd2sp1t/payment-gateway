using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentGatewayDbContext _dbContext;

    public UnitOfWork(PaymentGatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in _dbContext.ChangeTracker.Entries<DbEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.UpdatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
        {
            throw MapException(exception);
        }
    }

    private static PersistenceException MapException(DbUpdateException exception)
    {
        return new PersistenceException("Failed to save changes.", exception);
    }
}