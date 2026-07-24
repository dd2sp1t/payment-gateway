using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Abstractions.Persistence;

public interface IOperationRepository
{
    void Add(Operation operation);
    Task<Operation?> GetAsync(OperationId operationId, CancellationToken cancellationToken);
    Task UpdateAsync(Operation operation, CancellationToken cancellationToken);
}