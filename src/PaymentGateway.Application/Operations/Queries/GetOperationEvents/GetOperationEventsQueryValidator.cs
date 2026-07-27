using FluentValidation;

namespace PaymentGateway.Application.Operations.Queries.GetOperationEvents;

internal sealed class GetOperationEventsQueryValidator : AbstractValidator<GetOperationEventsQuery>
{
    public GetOperationEventsQueryValidator()
    {
        RuleFor(x => x.OperationId)
            .NotEmpty();
    }
}