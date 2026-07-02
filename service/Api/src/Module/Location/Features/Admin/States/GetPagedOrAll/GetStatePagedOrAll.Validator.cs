using Module.Location.Domain.States;

namespace Module.Location.Features.Admin.States.GetPagedOrAll;

public static partial class GetStatePagedOrAll
{
    public class QueryValidator : AbstractValidator<Query>
    {
        public QueryValidator()
        {
            RuleFor(expression: x => x.Parameters)
                .NotNull();

            When(predicate: x => x.Parameters is not null, action: () =>
            {
                RuleFor(expression: x => x.Parameters.PageNumber)
                    .Must(value => value.HasValue && value.Value >= 1)
                    .When(x => x.Parameters.PageNumber.HasValue)
                    .WithErrorCode("InvalidPage");

                RuleFor(expression: x => x.Parameters.PageSize)
                    .Must(value => value.HasValue && value.Value >= 1 && value.Value <= StateConstant.Constraints.MaxPageSize)
                    .When(x => x.Parameters.PageSize.HasValue)
                    .WithErrorCode("InvalidPageSize");

                RuleFor(expression: x => x.Parameters.Search)
                    .MaximumLength(StateConstant.Constraints.MaxSearchLength)
                    .When(x => x.Parameters.Search is not null)
                    .WithErrorCode("SearchTooLong");

                RuleFor(expression: x => x.Parameters.Filter)
                    .MaximumLength(StateConstant.Constraints.MaxFilterLength)
                    .When(x => x.Parameters.Filter is not null)
                    .WithErrorCode("FilterTooLong");
            });
        }
    }
}
