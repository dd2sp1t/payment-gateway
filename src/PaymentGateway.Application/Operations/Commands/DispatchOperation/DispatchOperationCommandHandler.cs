using MediatR;
using Microsoft.Extensions.Logging;
using PaymentGateway.Application.Abstractions.PaymentProvider;
using PaymentGateway.Application.Abstractions.PaymentProvider.Models;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Application.Abstractions.Persistence.Repositories;
using PaymentGateway.Application.Exceptions;
using PaymentGateway.Application.Extensions;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.Commands.DispatchOperation;

internal sealed class DispatchOperationCommandHandler : IRequestHandler<DispatchOperationCommand>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentProviderClient _paymentProviderClient;
    private readonly ILogger<DispatchOperationCommandHandler> _logger;

    public DispatchOperationCommandHandler(
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork,
        IPaymentProviderClient paymentProviderClient,
        ILogger<DispatchOperationCommandHandler> logger)
    {
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
        _paymentProviderClient = paymentProviderClient;
        _logger = logger;
    }

    public async Task Handle(DispatchOperationCommand request, CancellationToken cancellationToken)
    {
        var operation = await _operationRepository.GetAsync(request.OperationId, cancellationToken);

        if (operation is null)
        {
            _logger.LogWarning("Operation '{OperationId}' was not found.", request.OperationId);

            return;
        }

        if (operation.Status != OperationStatus.Processing)
        {
            _logger.LogDebug(
                "Operation '{OperationId}' is in status '{Status}'. Dispatch skipped.",
                operation.OperationId,
                operation.Status);

            return;
        }

        var providerRequest = new SubmitPaymentRequest(
            operation.OperationId,
            operation.Amount.ToInvariantString(),
            operation.Currency);

        var providerResponse = await _paymentProviderClient.SubmitAsync(providerRequest, cancellationToken);

        operation.AttachProviderPayment(providerResponse.ProviderPaymentId);

        await _operationRepository.UpdateAsync(operation, cancellationToken);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyException)
        {
            // callback may have already attached ProviderPaymentId and completed the operation
            _logger.LogInformation(
                "Operation '{OperationId}' was updated concurrently. Dispatch result ignored.",
                operation.OperationId);
        }
    }
}