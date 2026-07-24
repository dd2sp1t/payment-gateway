using Microsoft.EntityFrameworkCore;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Domain.Operations;
using PaymentGateway.Infrastructure.Persistence.Mappers;

namespace PaymentGateway.Infrastructure.Persistence.Repositories;

internal sealed class OperationRepository : IOperationRepository
{
    private readonly PaymentGatewayDbContext _dbContext;
    private readonly OperationMapper _mapper;

    public OperationRepository(PaymentGatewayDbContext dbContext, OperationMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public void Add(Operation operation)
    {
        var dbOperation = _mapper.ToEntity(operation);

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

        return _mapper.ToDomain(dbOperation);
    }

    public async Task UpdateAsync(Operation operation, CancellationToken cancellationToken)
    {
        var dbOperation = await _dbContext.Operations
            .SingleOrDefaultAsync(x => x.OperationId == operation.OperationId, cancellationToken);

        if (dbOperation is null)
        {
            throw new NotFoundException($"Operation '{operation.OperationId}' was not found.");
        }

        _mapper.Apply(operation, dbOperation);
    }
}