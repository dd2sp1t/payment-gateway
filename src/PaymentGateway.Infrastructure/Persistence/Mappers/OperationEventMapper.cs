using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Mappers;

internal sealed class OperationEventMapper
{
    public DbOperationEvent ToEntity(OperationEvent operationEvent)
    {
        return new DbOperationEvent
        {
            OperationId = operationEvent.OperationId,
            EventId = operationEvent.EventId,
            Type = operationEvent.Type,
            FromStatus = operationEvent.FromStatus,
            ToStatus = operationEvent.ToStatus,
            Message = operationEvent.Message,
            OccurredAt = operationEvent.OccurredAt
        };
    }
}