using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.Shared.Validators;

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
            .WithMessage(errorMessage: CountryResult.Failure.IsoCodeTooLong.Message);
    }
}