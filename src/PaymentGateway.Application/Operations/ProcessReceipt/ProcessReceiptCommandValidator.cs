using FluentValidation;

namespace PaymentGateway.Application.Operations.ProcessReceipt;

internal sealed class ProcessReceiptCommandValidator : AbstractValidator<ProcessReceiptCommand>
{
    public ProcessReceiptCommandValidator()
    {
        RuleFor(x => x.ProviderPaymentId)
            .NotEmpty();

        RuleFor(x => x.OperationId)
            .NotEmpty();

        RuleFor(x => x.Message)
            .NotEmpty();

        RuleFor(x => x.Result)
            .IsInEnum();

        RuleFor(x => x.OccurredAt)
            .NotEmpty();
    }
}