using FluentValidation;
using PaymentGateway.Domain.Operations;

namespace PaymentGateway.Application.Operations.CreateOperation;

internal sealed class CreateOperationCommandValidator : AbstractValidator<CreateOperationCommand>
{
    public CreateOperationCommandValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0);

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Equal(Currencies.Rub);

        RuleFor(x => x.Description)
            .NotEmpty();
    }
}