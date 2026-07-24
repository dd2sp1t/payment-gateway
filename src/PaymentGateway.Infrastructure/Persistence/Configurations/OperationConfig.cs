using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.ValueConverters;

namespace PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class OperationConfig : IEntityTypeConfiguration<DbOperation>
{
    private const int OperationIdMaxLength = 128;
    private const int CurrencyMaxLength = 3;
    private const int DescriptionMaxLength = 512;
    private const int StatusMaxLength = 16;

    public void Configure(EntityTypeBuilder<DbOperation> builder)
    {
        builder.ToTable("operations");

        builder
            .HasKey(x => x.OperationId)
            .HasName(DatabaseConstraints.OperationsPrimaryKey);

        builder
            .Property(x => x.OperationId)
            .HasMaxLength(OperationIdMaxLength)
            .IsRequired();

        builder
            .Property(x => x.ProviderPaymentId);

        builder
            .HasIndex(x => x.ProviderPaymentId)
            .IsUnique()
            .HasDatabaseName(DatabaseConstraints.OperationsProviderPaymentId);

        builder
            .Property(x => x.Amount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder
            .Property(x => x.Currency)
            .HasMaxLength(CurrencyMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(DescriptionMaxLength)
            .IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion(new OperationStatusConverter())
            .HasMaxLength(StatusMaxLength)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();

        builder
            .Property(x => x.Version)
            .IsRowVersion();
    }
}