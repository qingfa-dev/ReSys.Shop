using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.Shared.Validators;

public static partial class CountryValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyIsoCodeRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: CountryResult.Errors.IsoCodeRequired.Code)
            .WithMessage(errorMessage: CountryResult.Errors.IsoCodeRequired.Message)
            .MaximumLength(maximumLength: CountryConstant.Constraints.MaxIsoCodeLength)
            .WithErrorCode(errorCode: CountryResult.Errors.IsoCodeTooLong.Code)
            .WithMessage(errorMessage: CountryResult.Errors.IsoCodeTooLong.Message);
    }
}