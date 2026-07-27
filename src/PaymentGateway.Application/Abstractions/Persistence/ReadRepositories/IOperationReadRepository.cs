namespace PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;

using PaymentGateway.Domain.Operations;

public interface IOperationReadRepository
{
    Task<IReadOnlyList<OperationId>> GetProcessingOperationIdsAsync(int batchSize, CancellationToken cancellationToken);

    Task<bool> IsReceiptProcessedAsync(
        OperationId operationId,
        Guid providerPaymentId,
        ReceiptResult result,
        CancellationToken cancellationToken);
}