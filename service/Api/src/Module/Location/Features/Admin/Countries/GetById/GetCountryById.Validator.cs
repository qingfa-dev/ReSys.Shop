using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.GetById;

public static partial class GetCountryById
{
    // ============ QUERY VALIDATOR ============
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(expression: x => x.Id)
                .NotEmpty()
                .WithErrorCode(errorCode: CountryResult.Failure.IdRequired.Code)
                .WithMessage(errorMessage: CountryResult.Failure.IdRequired.Message);
        }
    }
}