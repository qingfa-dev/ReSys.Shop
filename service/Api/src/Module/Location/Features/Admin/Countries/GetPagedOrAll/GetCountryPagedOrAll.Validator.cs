using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.GetPagedOrAll;

public static partial class GetCountryPagedOrAll
{
    public class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Parameters)
                .NotNull();

            RuleFor(x => x.Parameters.PageNumber)
                .Must(value => value.HasValue && value.Value >= 1)
                .When(x => x.Parameters.PageNumber.HasValue)
                .WithErrorCode("InvalidPage");

            RuleFor(x => x.Parameters.PageSize)
                .Must(value => value.HasValue && value.Value >= 1 && value.Value <= CountryConstant.Constraints.Query.MaxPageSize)
                .When(x => x.Parameters.PageSize.HasValue)
                .WithErrorCode("InvalidPageSize");

            RuleFor(x => x.Parameters.Search)
                .MaximumLength(CountryConstant.Constraints.Query.MaxSearchLength)
                .When(x => x.Parameters.Search is not null)
                .WithErrorCode("SearchTooLong");

            RuleFor(x => x.Parameters.Filter)
                .MaximumLength(CountryConstant.Constraints.Query.MaxFilterLength)
                .When(x => x.Parameters.Filter is not null)
                .WithErrorCode("FilterTooLong");
        }
    }
}