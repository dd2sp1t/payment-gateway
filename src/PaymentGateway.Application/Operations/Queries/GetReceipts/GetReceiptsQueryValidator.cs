using FluentValidation;

namespace PaymentGateway.Application.Operations.Queries.GetOperation;

internal sealed class GetReceiptsQueryValidator : AbstractValidator<GetOperationQuery>
{
    public GetReceiptsQueryValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}