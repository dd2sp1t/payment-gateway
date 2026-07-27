using FluentValidation;

namespace PaymentGateway.Application.Operations.Queries.GetOperation;

internal sealed class GetOperationQueryValidator : AbstractValidator<GetOperationQuery>
{
    public GetOperationQueryValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}