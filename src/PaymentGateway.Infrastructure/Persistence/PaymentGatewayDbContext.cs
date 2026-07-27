using Microsoft.EntityFrameworkCore;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence;

internal sealed class PaymentGatewayDbContext : DbContext
{
    public PaymentGatewayDbContext(DbContextOptions<PaymentGatewayDbContext> options) : base(options)
    {
    }

    public DbSet<DbOperation> Operations => Set<DbOperation>();
    public DbSet<DbOperationEvent> OperationEvents => Set<DbOperationEvent>();
    public DbSet<DbReceipt> Receipts => Set<DbReceipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var assembly = typeof(PaymentGatewayDbContext).Assembly;
        modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        base.OnModelCreating(modelBuilder);
    }
}