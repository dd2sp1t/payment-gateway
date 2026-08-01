using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Infrastructure.Persistence.Entities;

internal sealed class DbOperation : DbEntity
{
    public string OperationId { get; set; } = null!;
    public Guid? ProviderPaymentId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = null!;
    public string Description { get; set; } = null!;
    public OperationStatus Status { get; set; }
    public int RetryCount { get; set; }
    public DateTimeOffset? NextDispatchAt { get; set; }
    public bool CanDispatch { get; set; }
    public long LastEventId { get; set; }
    public uint Version { get; set; }

    public ICollection<DbOperationEvent> OperationEvents { get; set; } = [];
    public ICollection<DbReceipt> Receipts { get; set; } = [];
}