using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.Mappers;

namespace PaymentGateway.Infrastructure.Persistence;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly PaymentGatewayDbContext _dbContext;
    private readonly PersistenceExceptionMapper _exceptionMapper;

    public UnitOfWork(PaymentGatewayDbContext dbContext, PersistenceExceptionMapper exceptionMapper)
    {
        _dbContext = dbContext;
        _exceptionMapper = exceptionMapper;
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
            throw _exceptionMapper.Map(exception);
        }
    }
}