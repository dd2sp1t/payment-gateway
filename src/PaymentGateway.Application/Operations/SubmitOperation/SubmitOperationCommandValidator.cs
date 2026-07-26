using FluentValidation;

namespace PaymentGateway.Application.Operations.SubmitOperation;

internal sealed class SubmitOperationCommandValidator : AbstractValidator<SubmitOperationCommand>
{
    public SubmitOperationCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}