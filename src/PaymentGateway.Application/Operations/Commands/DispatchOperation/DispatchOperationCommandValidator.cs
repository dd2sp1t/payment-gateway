using FluentValidation;

namespace PaymentGateway.Application.Operations.Commands.DispatchOperation;

internal sealed class DispatchOperationCommandValidator : AbstractValidator<DispatchOperationCommand>
{
    public DispatchOperationCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}