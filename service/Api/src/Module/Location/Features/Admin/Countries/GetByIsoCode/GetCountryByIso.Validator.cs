using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.GetByIsoCode;

public static partial class GetCountryByIso
{
    // ============ QUERY VALIDATOR ============
    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(expression: x => x.IsoCode)
                .NotEmpty()
                .WithErrorCode(errorCode: CountryResult.Failure.IsoCodeRequired.Code)
                .WithMessage(errorMessage: CountryResult.Failure.IsoCodeRequired.Message)
                .MaximumLength(maximumLength: CountryConstant.Constraints.MaxIsoCodeLength)
                .WithErrorCode(errorCode: CountryResult.Failure.IsoCodeTooLong.Code)
                .WithMessage(errorMessage: CountryResult.Failure.IsoCodeTooLong.Message);
        }
    }
}
