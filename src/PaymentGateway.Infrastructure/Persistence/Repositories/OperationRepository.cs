using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Entities;
using PaymentGateway.Infrastructure.Persistence.Mappers;

namespace PaymentGateway.Infrastructure.Persistence.Repositories;

internal sealed class OperationRepository : IOperationRepository
{
    private readonly PaymentGatewayDbContext _dbContext;
    private readonly OperationMapper _operationMapper;
    private readonly OperationEventMapper _operationEventMapper;

    public OperationRepository(
        PaymentGatewayDbContext dbContext,
        OperationMapper operationMapper,
        OperationEventMapper operationEventMapper)
    {
        _dbContext = dbContext;
        _operationMapper = operationMapper;
        _operationEventMapper = operationEventMapper;
    }

    public void Add(Operation operation)
    {
        var dbOperation = _operationMapper.ToEntity(operation);

        AddEvents(operation, dbOperation);

        _dbContext.Operations.Add(dbOperation);
    }

    public async Task<Operation?> GetAsync(OperationId operationId, CancellationToken cancellationToken)
    {
        var dbOperation = await _dbContext.Operations
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.OperationId == operationId, cancellationToken);

        if (dbOperation is null)
        {
            return null;
        }

        return _operationMapper.ToDomain(dbOperation);
    }

    public async Task UpdateAsync(Operation operation, CancellationToken cancellationToken)
    {
        var dbOperation = await _dbContext.Operations
            .SingleOrDefaultAsync(x => x.OperationId == operation.OperationId, cancellationToken);

        if (dbOperation is null)
        {
            throw new InvalidOperationException($"Operation '{operation.OperationId}' was not found during update.");
        }

        _operationMapper.Apply(operation, dbOperation);

        AddEvents(operation, dbOperation);
    }

    private void AddEvents(Operation operation, DbOperation dbOperation)
    {
        foreach (var @event in operation.UncommittedEvents)
        {
            dbOperation.OperationEvents.Add(_operationEventMapper.ToEntity(@event));
        }
    }
}