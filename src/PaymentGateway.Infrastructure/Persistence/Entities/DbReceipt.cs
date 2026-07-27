using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Infrastructure.Persistence.Entities;

internal sealed class DbReceipt : DbEntity
{
    public Guid ReceiptId { get; set; }
    public Guid ProviderPaymentId { get; set; }
    public string OperationId { get; set; } = null!;
    public ReceiptResult Result { get; set; }
    public string Message { get; set; } = null!;
    public DateTimeOffset OccurredAt { get; set; }

    public DbOperation Operation { get; set; } = null!;
}