using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Constants;
using PaymentGateway.Infrastructure.Persistence.Conventions;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.ValueConverters;

namespace PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class DbOperationConfig : IEntityTypeConfiguration<DbOperation>
{
    public void Configure(EntityTypeBuilder<DbOperation> builder)
    {
        builder
            .ToTable(DatabaseTables.Operations)
            .HasKey(x => x.OperationId)
            .HasName(ConstraintNames.OperationsPrimaryKey);

        builder
            .Property(x => x.OperationId)
            .HasMaxLength(DbOperationConstants.OperationIdLength)
            .IsRequired();

        builder
            .Property(x => x.ProviderPaymentId)
            .IsRequired(false);

        builder
            .HasIndex(x => x.ProviderPaymentId)
            .IsUnique()
            .HasDatabaseName(ConstraintNames.OperationsProviderPaymentIdUnique);

        builder
            .Property(x => x.Amount)
            .HasPrecision(
                precision: DbOperationConstants.AmountPrecision,
                scale: DbOperationConstants.AmountScale)
            .IsRequired();

        builder
            .Property(x => x.Currency)
            .HasMaxLength(DbOperationConstants.CurrencyLength)
            .IsRequired();

        builder
            .Property(x => x.Description)
            .HasMaxLength(DbOperationConstants.DescriptionLength)
            .IsRequired();

        builder
            .Property(x => x.Status)
            .HasConversion(new EnumToStringConverter<OperationStatus>())
            .HasMaxLength(DbOperationConstants.StatusLength)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();

        builder
            .Property(x => x.LastEventId)
            .IsRequired();

        builder
            .Property(x => x.Version)
            .IsRowVersion();

        builder
            .HasMany(x => x.OperationEvents)
            .WithOne(x => x.Operation)
            .HasForeignKey(x => x.OperationId)
            .HasConstraintName(ConstraintNames.OperationEventsOperationForeignKey);
    }
}