using System.Globalization;
using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.Diagnostics;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Application.Operations.Models;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.CreateOperation;

internal sealed class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, OperationResponse>
{
    private readonly ILogger<CreateOperationCommandHandler> _logger;
    private readonly IMetrics _metrics;
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOperationCommandHandler(
        ILogger<CreateOperationCommandHandler> logger,
        IMetrics metrics,
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _metrics = metrics;
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResponse> Handle(CreateOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = Operation.Create(
            (OperationId)request.OperationId,
            amount: decimal.Parse(request.Amount, NumberStyles.Number, CultureInfo.InvariantCulture),
            request.Currency,
            request.Description);

        _operationRepository.Add(operation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _metrics.OperationCreated();

        _logger.LogInformation(
            "Operation created. OperationId={OperationId} Amount={Amount} Currency={Currency}",
            operation.OperationId,
            operation.Amount,
            operation.Currency);

        operation.ClearUncommittedEvents();

        return new OperationResponse(
            operation.OperationId,
            operation.Amount.ToInvariantString(),
            operation.Currency,
            operation.Description,
            operation.Status,
            operation.ProviderPaymentId,
            operation.RetryCount,
            operation.NextDispatchAt);
    }
}