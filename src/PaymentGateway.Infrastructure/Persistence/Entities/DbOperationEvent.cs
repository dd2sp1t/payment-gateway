using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Infrastructure.Persistence.Entities;

internal sealed class DbOperationEvent : DbEntity
{
    public string OperationId { get; set; } = null!;

    public long EventId { get; set; }

    public OperationEventType Type { get; set; }

    public OperationStatus? FromStatus { get; set; }

    public OperationStatus ToStatus { get; set; }

    public string Message { get; set; } = null!;

    public DateTimeOffset OccurredAt { get; set; }

    public DbOperation Operation { get; set; } = null!;
}