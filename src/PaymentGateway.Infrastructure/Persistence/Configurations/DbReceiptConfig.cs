using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Constants;
using PaymentGateway.Infrastructure.Persistence.Conventions;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.ValueConverters;

namespace PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class DbReceiptConfig : IEntityTypeConfiguration<DbReceipt>
{
    public void Configure(EntityTypeBuilder<DbReceipt> builder)
    {
        builder
            .ToTable(DatabaseTables.Receipts)
            .HasKey(x => x.ReceiptId)
            .HasName(ConstraintNames.ReceiptsPrimaryKey);

        builder
            .Property(x => x.ReceiptId)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(x => x.ProviderPaymentId)
            .IsRequired();

        builder
            .Property(x => x.OperationId)
            .HasMaxLength(DbReceiptConstants.OperationIdLength)
            .IsRequired();

        builder
            .Property(x => x.Result)
            .HasConversion(new EnumToStringConverter<ReceiptResult>())
            .HasMaxLength(DbReceiptConstants.ResultLength)
            .IsRequired();

        builder
            .Property(x => x.Message)
            .HasMaxLength(DbReceiptConstants.MessageLength)
            .IsRequired();

        builder
            .Property(x => x.OccurredAt)
            .IsRequired();

        builder
            .Property(x => x.CreatedAt)
            .IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();

        builder
            .HasOne(x => x.Operation)
            .WithMany(x => x.Receipts)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new
            {
                x.OperationId,
                x.ProviderPaymentId,
                x.Result
            })
            .IsUnique()
            .HasDatabaseName(ConstraintNames.ReceiptsOperationProviderPaymentResultUnique);
    }
}