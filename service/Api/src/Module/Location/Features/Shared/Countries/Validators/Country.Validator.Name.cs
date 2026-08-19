using Module.Location.Domain.Countries;

namespace Module.Location.Features.Shared.Countries.Validators;

public static partial class CountryValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: CountryResult.Failure.NameRequired.Code)
            .WithMessage(errorMessage: CountryResult.Failure.NameRequired.Message)
            .MaximumLength(maximumLength: CountryConstant.Constraints.MaxNameLength)
            .WithErrorCode(errorCode: CountryResult.Failure.NameTooLong.Code)
            .WithMessage(errorMessage: CountryResult.Failure.NameTooLong.Message);
    }
}