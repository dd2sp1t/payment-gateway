namespace PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;

using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
using PaymentGateway.Domain.Operations;

public interface IOperationReadRepository
{
    Task<IReadOnlyList<OperationId>> GetProcessingOperationIdsAsync(int batchSize, CancellationToken cancellationToken);

    Task<bool> IsReceiptProcessedAsync(
        OperationId operationId,
        Guid providerPaymentId,
        ReceiptResult result,
        CancellationToken cancellationToken);

    Task<OperationReadModel?> GetOperationAsync(OperationId operationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<OperationEventReadModel>> GetOperationEventsAsync(
        OperationId operationId,
        CancellationToken cancellationToken);
}