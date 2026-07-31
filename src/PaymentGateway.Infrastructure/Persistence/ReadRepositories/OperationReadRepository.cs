using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence.ReadModels;
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
        var now = DateTimeOffset.UtcNow;

        return await _dbContext.Operations
            .AsNoTracking()
            .Where(x => x.Status == OperationStatus.Processing
                    && (x.NextDispatchAt == null || x.NextDispatchAt <= now))
            .OrderBy(x => x.NextDispatchAt ?? x.UpdatedAt)
            .ThenBy(x => x.UpdatedAt)
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

    public async Task<OperationReadModel?> GetOperationAsync(
        OperationId operationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Operations
            .Where(x => x.OperationId == operationId)
            .Select(x => new OperationReadModel(
                (OperationId)x.OperationId,
                x.Amount,
                x.Currency,
                x.Description,
                x.Status,
                x.ProviderPaymentId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OperationEventReadModel>> GetOperationEventsAsync(
        OperationId operationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.OperationEvents
            .Where(x => x.OperationId == operationId)
            .OrderBy(x => x.EventId)
            .Select(x => new OperationEventReadModel(
                x.EventId,
                x.Type,
                x.FromStatus,
                x.ToStatus,
                x.Message,
                x.OccurredAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<TimeSpan?> GetOldestProcessingAgeAsync(CancellationToken cancellationToken)
    {
        var updatedAt = await _dbContext.Operations
            .Where(x => x.Status == OperationStatus.Processing)
            .OrderBy(x => x.UpdatedAt)
            .Select(x => x.UpdatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (updatedAt == default)
        {
            return null;
        }

        return DateTimeOffset.UtcNow - updatedAt;
    }

    public async Task<IReadOnlyList<ReceiptReadModel>> GetReceiptsAsync(
        OperationId operationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Receipts
            .AsNoTracking()
            .Where(x => x.OperationId == operationId)
            .OrderBy(x => x.CreatedAt)
            .Select(x => new ReceiptReadModel(
                x.ProviderPaymentId,
                x.Result,
                x.Message,
                x.OccurredAt))
            .ToListAsync(cancellationToken);
    }
}