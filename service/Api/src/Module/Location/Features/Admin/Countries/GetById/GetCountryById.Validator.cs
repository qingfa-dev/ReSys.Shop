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
                .WithErrorCode(errorCode: CountryResult.Errors.IdRequired.Code)
                .WithMessage(errorMessage: CountryResult.Errors.IdRequired.Message);
        }
    }
}
