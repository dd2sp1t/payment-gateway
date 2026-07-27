using System.Globalization;
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
            .NotEmpty()
            .Must(BeValidAmount)
            .WithMessage("Amount must be a valid decimal number greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .Equal(Currencies.Rub);

        RuleFor(x => x.Description)
            .NotEmpty();
    }

    private static bool BeValidAmount(string amount)
    {
        return decimal.TryParse(amount, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) && value > 0;
    }
}