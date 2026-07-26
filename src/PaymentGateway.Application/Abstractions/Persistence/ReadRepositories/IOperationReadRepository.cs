namespace PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;

using PaymentGateway.Application.Abstractions.Persistence.ReadModels;

public interface IOperationReadRepository
{
    Task<IReadOnlyList<OperationDispatchModel>> GetProcessingOperationsAsync(
        int batchSize,
        CancellationToken cancellationToken);
}