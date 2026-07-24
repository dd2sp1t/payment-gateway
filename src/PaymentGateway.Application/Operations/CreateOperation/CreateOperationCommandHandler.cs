using MediatR;
using PaymentGateway.Application.Abstractions.Persistence;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.CreateOperation;

internal sealed class CreateOperationCommandHandler : IRequestHandler<CreateOperationCommand, CreateOperationResponse>
{
    private readonly IOperationRepository _operationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOperationCommandHandler(
        IOperationRepository operationRepository,
        IUnitOfWork unitOfWork)
    {
        _operationRepository = operationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateOperationResponse> Handle(
        CreateOperationCommand request,
        CancellationToken cancellationToken)
    {
        var operation = Operation.Create(
            (OperationId)request.OperationId,
            request.Amount,
            request.Currency,
            request.Description);

        _operationRepository.Add(operation);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateOperationResponse(operation.OperationId, operation.Status);
    }
}