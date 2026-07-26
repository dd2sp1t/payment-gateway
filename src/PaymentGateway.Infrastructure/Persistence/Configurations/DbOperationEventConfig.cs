using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Constants;
using PaymentGateway.Infrastructure.Persistence.Conventions;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.ValueConverters;

namespace PaymentGateway.Infrastructure.Persistence.Configurations;

internal sealed class DbOperationEventConfig : IEntityTypeConfiguration<DbOperationEvent>
{
    public void Configure(EntityTypeBuilder<DbOperationEvent> builder)
    {
        builder
            .ToTable(DatabaseTables.OperationEvents)
            .HasKey(x => new { x.OperationId, x.EventId })
            .HasName(ConstraintNames.OperationEventsPrimaryKey);

        builder
            .Property(x => x.EventId)
            .ValueGeneratedNever()
            .IsRequired();

        builder
            .Property(x => x.Type)
            .HasConversion(new EnumToStringConverter<OperationEventType>())
            .HasMaxLength(DbOperationEventConstants.TypeLength)
            .IsRequired();

        builder
            .Property(x => x.FromStatus)
            .HasConversion(new EnumToStringConverter<OperationStatus>())
            .HasMaxLength(DbOperationEventConstants.StatusLength)
            .IsRequired(false);

        builder
            .Property(x => x.ToStatus)
            .HasConversion(new EnumToStringConverter<OperationStatus>())
            .HasMaxLength(DbOperationEventConstants.StatusLength)
            .IsRequired();

        builder
            .Property(x => x.Message)
            .HasMaxLength(DbOperationEventConstants.MessageLength)
            .IsRequired();

        builder
            .Property(x => x.OccurredAt)
            .IsRequired();

        builder
            .HasOne(x => x.Operation)
            .WithMany(x => x.OperationEvents)
            .HasForeignKey(x => x.OperationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}