using FluentValidation;

namespace PaymentGateway.Application.Operations.DispatchOperation;

internal sealed class DispatchOperationCommandValidator : AbstractValidator<DispatchOperationCommand>
{
    public DispatchOperationCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotNull();
    }
}