namespace PaymentGateway.Infrastructure.Persistence.Entities;

internal abstract class DbEntity
{
    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }
}