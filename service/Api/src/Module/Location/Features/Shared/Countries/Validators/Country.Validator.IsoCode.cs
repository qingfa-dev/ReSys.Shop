using Module.Location.Domain.Countries;

namespace Module.Location.Features.Shared.Countries.Validators;

public static partial class CountryValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyIsoCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: CountryResult.Failure.IsoCodeRequired.Code)
            .WithMessage(errorMessage: CountryResult.Failure.IsoCodeRequired.Message)
            .MaximumLength(maximumLength: CountryConstant.Constraints.MaxIsoCodeLength)
            .WithErrorCode(errorCode: CountryResult.Failure.IsoCodeTooLong.Code)
            .WithMessage(errorMessage: CountryResult.Failure.IsoCodeTooLong.Message)
            .Matches("^[A-Z]{2,3}$")
            .WithErrorCode("Country.IsoCode.InvalidFormat")
            .WithMessage("ISO code must be 2-3 uppercase letters.");
    }
}