using Module.Location.Domain.Countries;

namespace Module.Location.Features.Admin.Countries.Shared.Validators;

public static partial class CountryValidator
{
    internal static IRuleBuilderOptions<T, string?> ApplyNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty()
            .WithErrorCode(errorCode: CountryResult.Errors.NameRequired.Code)
            .WithMessage(errorMessage: CountryResult.Errors.NameRequired.Message)
            .MaximumLength(maximumLength: CountryConstant.Constraints.MaxNameLength)
            .WithErrorCode(errorCode: CountryResult.Errors.NameTooLong.Code)
            .WithMessage(errorMessage: CountryResult.Errors.NameTooLong.Message);
    }
}