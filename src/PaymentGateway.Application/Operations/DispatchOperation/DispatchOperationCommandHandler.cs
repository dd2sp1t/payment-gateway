using MediatR;

namespace PaymentGateway.Application.Operations.DispatchOperation;

internal sealed class DispatchOperationCommandHandler : IRequestHandler<DispatchOperationCommand>
{
    public Task Handle(DispatchOperationCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}