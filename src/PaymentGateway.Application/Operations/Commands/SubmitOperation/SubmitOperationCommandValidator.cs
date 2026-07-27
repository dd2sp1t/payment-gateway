using FluentValidation;

namespace PaymentGateway.Application.Operations.Commands.SubmitOperation;

internal sealed class SubmitOperationCommandValidator : AbstractValidator<SubmitOperationCommand>
{
    public SubmitOperationCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}