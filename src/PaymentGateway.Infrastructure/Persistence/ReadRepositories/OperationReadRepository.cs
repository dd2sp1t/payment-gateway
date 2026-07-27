using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence.ReadRepositories;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Infrastructure.Persistence.ReadRepositories;

internal sealed class OperationReadRepository : IOperationReadRepository
{
    private readonly PaymentGatewayDbContext _dbContext;

    public OperationReadRepository(PaymentGatewayDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<OperationId>> GetProcessingOperationIdsAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Operations
            .AsNoTracking()
            .Where(x => x.Status == OperationStatus.Processing)
            .OrderBy(x => x.UpdatedAt)
            .Take(batchSize)
            .Select(x => (OperationId)x.OperationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsReceiptProcessedAsync(
        OperationId operationId,
        Guid providerPaymentId,
        ReceiptResult result,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Receipts.AnyAsync(
            x => x.OperationId == operationId
                && x.ProviderPaymentId == providerPaymentId
                && x.Result == result,
             cancellationToken);
    }
}